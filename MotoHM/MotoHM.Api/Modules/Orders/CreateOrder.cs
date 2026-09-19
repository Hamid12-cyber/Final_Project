using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Entites;
using MotoHM.Api.Shared.Exceptions.Common;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Orders;

public static class CreateOrder
{
    public record CreateOrderCommand(int UserId, string ShippingAddress, string ContactPhone) : IRequest<int>;

    public class Handler : IRequestHandler<CreateOrderCommand, int>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var cart = await _db.Carts
                .Include(c => c.Items).ThenInclude(i => i.Motorcycle)
                .Include(c => c.Items).ThenInclude(i => i.Part)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            if (cart is null || cart.Items.Count == 0)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Səbət boşdur, sifariş yaradıla bilməz.");

            var order = new OrderEntity
            {
                UserId = request.UserId,
                ShippingAddress = request.ShippingAddress,
                ContactPhone = request.ContactPhone,
                TotalAmount = 0
            };

            decimal total = 0;

            foreach (var cartItem in cart.Items)
            {
                var unitPrice = cartItem.Motorcycle?.Price ?? cartItem.Part?.Price
                    ?? throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                        "Səbətdəki bir məhsulun qiyməti tapılmadı.");

                order.Items.Add(new OrderItemEntity
                {
                    MotorcycleId = cartItem.MotorcycleId,
                    PartId = cartItem.PartId,
                    Quantity = cartItem.Quantity,
                    UnitPriceAtOrderTime = unitPrice
                });

                total += unitPrice * cartItem.Quantity;
            }

            order.TotalAmount = total;

            _db.Orders.Add(order);

            // Sifariş yarandıqdan sonra səbəti təmizləyirik
            _db.CartItems.RemoveRange(cart.Items);

            await _db.SaveChangesAsync(cancellationToken);

            return order.Id;
        }
    }

    public record CreateOrderBody(string ShippingAddress, string ContactPhone);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (CreateOrderBody body, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.GetUserId();
            var command = new CreateOrderCommand(userId, body.ShippingAddress, body.ContactPhone);
            var id = await sender.Send(command);
            return Results.Created($"/api/orders/{id}", new { id });
        })
        .RequireAuthorization()
        .WithName("CreateOrder")
        .WithTags("Orders");
    }
}