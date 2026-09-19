using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Cart;

public static class RemoveFromCart
{
    public record Command(int CartItemId) : IRequest<bool>;

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var item = await _db.CartItems.FirstOrDefaultAsync(i => i.Id == request.CartItemId, cancellationToken);

            if (item is null)
                return false;

            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/cart/items/{cartItemId:int}", async (int cartItemId, ISender sender) =>
        {
            var success = await sender.Send(new Command(cartItemId));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("RemoveFromCart")
        .WithTags("Cart");
    }
}