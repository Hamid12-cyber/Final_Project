using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Entites;
using MotoHM.Api.Shared.Exceptions.Common;

namespace MotoHM.Api.Modules.Cart;

public static class AddToCart
{
    public record AddToCartCommand(int UserId, int? MotorcycleId, int? PartId, int Quantity) : IRequest;

    public class Handler : IRequestHandler<AddToCartCommand>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            if ((request.MotorcycleId is null && request.PartId is null) ||
                (request.MotorcycleId is not null && request.PartId is not null))
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Ya MotorcycleId, ya da PartId göndərilməlidir (ikisi birdən yox).");

            if (request.Quantity < 1)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Miqdar 1-dən az ola bilməz.");

            var cart = await _db.Carts.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            if (cart is null)
            {
                cart = new CartEntity { UserId = request.UserId };
                _db.Carts.Add(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i =>
                i.MotorcycleId == request.MotorcycleId && i.PartId == request.PartId);

            if (existingItem is not null)
            {
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItemEntity
                {
                    MotorcycleId = request.MotorcycleId,
                    PartId = request.PartId,
                    Quantity = request.Quantity
                });
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public record AddToCartBody(int? MotorcycleId, int? PartId, int Quantity);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cart/items", async (AddToCartBody body, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await sender.Send(new AddToCartCommand(userId, body.MotorcycleId, body.PartId, body.Quantity));
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("AddToCart")
        .WithTags("Cart");
    }
}