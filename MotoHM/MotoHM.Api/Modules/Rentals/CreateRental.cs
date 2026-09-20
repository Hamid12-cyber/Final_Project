using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Entites;
using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Exceptions.Common;

namespace MotoHM.Api.Modules.Rentals;

public static class CreateRental
{
    public record CreateRentalCommand(int MotorcycleId, DateTime StartDate, DateTime EndDate,
        RentalPeriod Period, string CustomerName, string CustomerPhone) : IRequest<int>;

    public class Handler : IRequestHandler<CreateRentalCommand, int>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<int> Handle(CreateRentalCommand request, CancellationToken cancellationToken)
        {
            var motorcycle = await _db.Motorcycles
                .FirstOrDefaultAsync(m => m.Id == request.MotorcycleId, cancellationToken);

            if (motorcycle is null)
                throw new AppException("NOT_FOUND", StatusCodes.Status404NotFound,
                    $"Motorcycle (Id={request.MotorcycleId}) tapılmadı.");

            if (!motorcycle.IsForRent)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Bu motosiklet kirayə üçün deyil.");

            var totalPrice = CalculatePrice(request.StartDate, request.EndDate, request.Period);

            var entity = new RentalEntity
            {
                MotorcycleId = request.MotorcycleId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Period = request.Period,
                TotalPrice = totalPrice,
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone
            };

            _db.Rentals.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }

        private static decimal CalculatePrice(DateTime start, DateTime end, RentalPeriod period)
        {
            var days = (end - start).Days;
            if (days < 1) days = 1;

            return period switch
            {
                RentalPeriod.Daily => days * 50m,
                RentalPeriod.Weekly => (days / 7.0m) * 300m,
                RentalPeriod.Monthly => (days / 30.0m) * 1000m,
                _ => throw new ArgumentOutOfRangeException(nameof(period))
            };
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/rentals", async (CreateRentalCommand command, ISender sender) =>
        {
            var id = await sender.Send(command);
            return Results.Created($"/api/rentals/{id}", new { id });
        })
        .WithName("CreateRental")
        .WithTags("Rentals");
    }
    public class Validator : AbstractValidator<CreateRentalCommand>
    {
        public Validator()
        {
            RuleFor(x => x.MotorcycleId).GreaterThan(0);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
            RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.CustomerPhone).NotEmpty().MaximumLength(30);
        }
    }
}