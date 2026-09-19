using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Exceptions.Common;

namespace MotoHM.Api.Modules.Rentals;

public static class UpdateRental
{
    public record UpdateRentalCommand(int Id, DateTime StartDate, DateTime EndDate,
        RentalPeriod Period, string CustomerName, string CustomerPhone) : IRequest<bool>;

    public class Handler : IRequestHandler<UpdateRentalCommand, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(UpdateRentalCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.Rentals.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (entity is null)
                return false;

            if (request.EndDate <= request.StartDate)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Bitmə tarixi başlanğıc tarixindən sonra olmalıdır.");

            entity.StartDate = request.StartDate;
            entity.EndDate = request.EndDate;
            entity.Period = request.Period;
            entity.CustomerName = request.CustomerName;
            entity.CustomerPhone = request.CustomerPhone;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/rentals/{id:int}", async (int id, UpdateRentalBody body, ISender sender) =>
        {
            var command = new UpdateRentalCommand(id, body.StartDate, body.EndDate,
                body.Period, body.CustomerName, body.CustomerPhone);

            var success = await sender.Send(command);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateRental")
        .WithTags("Rentals");
    }

    public record UpdateRentalBody(DateTime StartDate, DateTime EndDate,
        RentalPeriod Period, string CustomerName, string CustomerPhone);
}