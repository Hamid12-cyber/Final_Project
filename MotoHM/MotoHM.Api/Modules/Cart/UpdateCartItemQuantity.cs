using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Shared.Exceptions.Common;

namespace MotoHM.Api.Modules.Cart;

public static class UpdateCartItemQuantity
{
    public record Command(int CartItemId, int Quantity) : IRequest<bool>;

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.Quantity < 1)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Miqdar 1-dən az ola bilməz.");

            var item = await _db.CartItems.FirstOrDefaultAsync(i => i.Id == request.CartItemId, cancellationToken);

            if (item is null)
                return false;

            item.Quantity = request.Quantity;
            item.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public record UpdateQuantityBody(int Quantity);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/cart/items/{cartItemId:int}", async (int cartItemId, UpdateQuantityBody body, ISender sender) =>
        {
            var success = await sender.Send(new Command(cartItemId, body.Quantity));
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