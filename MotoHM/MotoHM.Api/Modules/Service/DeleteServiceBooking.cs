using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Service;

public static class DeleteServiceBooking
{
    public record DeleteServiceBookingCommand(int Id) : IRequest<bool>;

    public class Handler : IRequestHandler<DeleteServiceBookingCommand, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(DeleteServiceBookingCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.ServiceBookings.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (entity is null)
                return false;

            _db.ServiceBookings.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/service-bookings/{id:int}", async (int id, ISender sender) =>
        {
            var success = await sender.Send(new DeleteServiceBookingCommand(id));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteServiceBooking")
        .WithTags("Service");
    }
}