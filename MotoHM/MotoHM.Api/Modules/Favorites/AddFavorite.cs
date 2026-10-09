using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Entites;
using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Exceptions.Common;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Favorites;

public static class AddFavorite
{
    public record Command(int UserId, int MotorcycleId) : IRequest;

    public class Handler : IRequestHandler<Command>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var exists = await _db.Motorcycles.AnyAsync(
                m => m.Id == request.MotorcycleId && m.Status == ApprovalStatus.Approved, cancellationToken);

            if (!exists)
                throw new AppException("NOT_FOUND", StatusCodes.Status404NotFound,
                    $"Motorcycle (Id={request.MotorcycleId}) tapılmadı.");

            // Artıq favoridədirsə, heç nə etmirik (təkrar əlavə xəta vermir)
            var already = await _db.Favorites.AnyAsync(
                f => f.UserId == request.UserId && f.MotorcycleId == request.MotorcycleId, cancellationToken);

            if (already)
                return;

            _db.Favorites.Add(new FavoriteEntity
            {
                UserId = request.UserId,
                MotorcycleId = request.MotorcycleId
            });

            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/favorites/{motorcycleId:int}", async (int motorcycleId, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.GetUserId();
            await sender.Send(new Command(userId, motorcycleId));
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("AddFavorite")
        .WithTags("Favorites");
    }
}
