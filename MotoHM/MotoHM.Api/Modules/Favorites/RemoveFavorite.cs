using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Favorites;

public static class RemoveFavorite
{
    public record Command(int UserId, int MotorcycleId) : IRequest;

    public class Handler : IRequestHandler<Command>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var favorite = await _db.Favorites.FirstOrDefaultAsync(
                f => f.UserId == request.UserId && f.MotorcycleId == request.MotorcycleId, cancellationToken);

            // Favoridə yoxdursa da xəta vermirik, nəticə eynidir
            if (favorite is null)
                return;

            _db.Favorites.Remove(favorite);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/favorites/{motorcycleId:int}", async (int motorcycleId, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.GetUserId();
            await sender.Send(new Command(userId, motorcycleId));
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("RemoveFavorite")
        .WithTags("Favorites");
    }
}
