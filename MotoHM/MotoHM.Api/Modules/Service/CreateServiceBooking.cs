using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Entites;
using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Exceptions.Common;

namespace MotoHM.Api.Modules.Service;

public static class CreateServiceBooking
{
    public record CreateServiceBookingCommand(int MotorcycleId, ServiceType Type, DateTime ScheduledDate,
        string CustomerName, string CustomerPhone, string? Notes) : IRequest<int>;

    public class Handler : IRequestHandler<CreateServiceBookingCommand, int>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<int> Handle(CreateServiceBookingCommand request, CancellationToken cancellationToken)
        {
            var motorcycleExists = await _db.Motorcycles.AnyAsync(m => m.Id == request.MotorcycleId, cancellationToken);
            if (!motorcycleExists)
                throw new AppException("NOT_FOUND", StatusCodes.Status404NotFound,
                    $"Motorcycle (Id={request.MotorcycleId}) tapılmadı.");

            if (request.ScheduledDate < DateTime.UtcNow.Date)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Bron tarixi keçmişdə ola bilməz.");

            var entity = new ServiceBookingEntity
            {
                MotorcycleId = request.MotorcycleId,
                Type = request.Type,
                ScheduledDate = request.ScheduledDate,
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone,
                Notes = request.Notes
            };

            _db.ServiceBookings.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/service-bookings", async (CreateServiceBookingCommand command, ISender sender) =>
        {
            var id = await sender.Send(command);
            return Results.Created($"/api/service-bookings/{id}", new { id });
        })
        .WithName("CreateServiceBooking")
        .WithTags("Service");
    }
    public class Validator : AbstractValidator<CreateServiceBookingCommand>
    {
        public Validator()
        {
            RuleFor(x => x.MotorcycleId).GreaterThan(0);
            RuleFor(x => x.ScheduledDate).NotEmpty();
            RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.CustomerPhone).NotEmpty().MaximumLength(30);
            RuleFor(x => x.Notes).MaximumLength(500);
        }
    }
}