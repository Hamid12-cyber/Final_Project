using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Reviews;

public static class DeleteReview
{
    public record Command(int Id, int UserId, bool IsAdmin) : IRequest<bool>;

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            // Rəyi yalnız müəllifi və ya admin silə bilər
            var review = await _db.Reviews.FirstOrDefaultAsync(
                r => r.Id == request.Id && (request.IsAdmin || r.UserId == request.UserId), cancellationToken);

            if (review is null)
                return false;

            _db.Reviews.Remove(review);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/reviews/{id:int}", async (int id, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.GetUserId();
            var success = await sender.Send(new Command(id, userId, user.IsInRole("Admin")));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("DeleteReview")
        .WithTags("Reviews");
    }
}