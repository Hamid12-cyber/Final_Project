using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Exceptions.Common;

namespace MotoHM.Api.Modules.Service;

public static class UpdateServiceBooking
{
    public record UpdateServiceBookingCommand(int Id, ServiceType Type, DateTime ScheduledDate,
        string CustomerName, string CustomerPhone, string? Notes) : IRequest<bool>;

    public class Handler : IRequestHandler<UpdateServiceBookingCommand, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(UpdateServiceBookingCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.ServiceBookings.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (entity is null)
                return false;

            if (request.ScheduledDate < DateTime.UtcNow.Date)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Bron tarixi keçmişdə ola bilməz.");

            var slotTaken = await _db.ServiceBookings.AnyAsync(s =>
                s.Id != request.Id && s.ScheduledDate == request.ScheduledDate, cancellationToken);

            if (slotTaken)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Seçilən vaxt artıq tutulub.");

            entity.Type = request.Type;
            entity.ScheduledDate = request.ScheduledDate;
            entity.CustomerName = request.CustomerName;
            entity.CustomerPhone = request.CustomerPhone;
            entity.Notes = request.Notes;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/service-bookings/{id:int}", async (int id, UpdateServiceBookingBody body, ISender sender) =>
        {
            var command = new UpdateServiceBookingCommand(id, body.Type, body.ScheduledDate,
                body.CustomerName, body.CustomerPhone, body.Notes);

            var success = await sender.Send(command);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateServiceBooking")
        .WithTags("Service");
    }

    public record UpdateServiceBookingBody(ServiceType Type, DateTime ScheduledDate,
        string CustomerName, string CustomerPhone, string? Notes);

    public class Validator : AbstractValidator<UpdateServiceBookingCommand>
    {
        public Validator()
        {
            RuleFor(x => x.ScheduledDate).NotEmpty();
            RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.CustomerPhone).NotEmpty().MaximumLength(30);
            RuleFor(x => x.Notes).MaximumLength(500);
        }
    }
}