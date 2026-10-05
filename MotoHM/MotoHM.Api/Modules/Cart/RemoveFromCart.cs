using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Cart;

public static class RemoveFromCart
{
    public record Command(int CartItemId, int UserId) : IRequest<bool>;

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            // Yalnız öz səbətindəki item silinə bilər
            var item = await _db.CartItems.FirstOrDefaultAsync(
                i => i.Id == request.CartItemId && i.Cart.UserId == request.UserId, cancellationToken);

            if (item is null)
                return false;

            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/cart/items/{cartItemId:int}", async (int cartItemId, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.GetUserId();
            var success = await sender.Send(new Command(cartItemId, userId));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("RemoveFromCart")
        .WithTags("Cart");
    }
}