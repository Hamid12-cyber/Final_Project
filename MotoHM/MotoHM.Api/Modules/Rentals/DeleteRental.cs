using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Rentals;

public static class DeleteRental
{
    public record DeleteRentalCommand(int Id) : IRequest<bool>;

    public class Handler : IRequestHandler<DeleteRentalCommand, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(DeleteRentalCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.Rentals.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (entity is null)
                return false;

            _db.Rentals.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/rentals/{id:int}", async (int id, ISender sender) =>
        {
            var success = await sender.Send(new DeleteRentalCommand(id));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteRental")
        .WithTags("Rentals");
    }
}