using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Shared.Exceptions.Common;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Cart;

public static class UpdateCartItemQuantity
{
    public record Command(int CartItemId, int UserId, int Quantity) : IRequest<bool>;

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.Quantity < 1)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Miqdar 1-dən az ola bilməz.");

            // Yalnız öz səbətindəki item dəyişdirilə bilər
            var item = await _db.CartItems
                .Include(i => i.Accessory)
                .FirstOrDefaultAsync(i => i.Id == request.CartItemId && i.Cart.UserId == request.UserId, cancellationToken);

            if (item is null)
                return false;

            // Aksesuar üçün stokdan çox olmasın
            if (item.Accessory is not null && request.Quantity > item.Accessory.StockQty)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    $"Stokda yalnız {item.Accessory.StockQty} ədəd var.");

            item.Quantity = request.Quantity;
            item.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public record UpdateQuantityBody(int Quantity);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/cart/items/{cartItemId:int}", async (int cartItemId, UpdateQuantityBody body, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.GetUserId();
            var success = await sender.Send(new Command(cartItemId, userId, body.Quantity));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("UpdateCartItemQuantity")
        .WithTags("Cart");
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }
}