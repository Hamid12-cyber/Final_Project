using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Entites;
using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Exceptions.Common;
using MotoHM.Api.Shared.Extensions;
using System.Security.Claims;

namespace MotoHM.Api.Modules.Cart;

public static class AddToCart
{
    public record AddToCartCommand(int UserId, int? MotorcycleId, int? PartId, int? AccessoryId, int Quantity) : IRequest;

    public class Handler : IRequestHandler<AddToCartCommand>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            var providedCount = new[] { request.MotorcycleId, request.PartId, request.AccessoryId }
                .Count(x => x.HasValue);

            if (providedCount != 1)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Ya MotorcycleId, ya PartId, ya da AccessoryId göndərilməlidir (yalnız biri).");

            if (request.Quantity < 1)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Miqdar 1-dən az ola bilməz.");

            var cart = await _db.Carts.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            var existingItem = cart?.Items.FirstOrDefault(i =>
                i.MotorcycleId == request.MotorcycleId &&
                i.PartId == request.PartId &&
                i.AccessoryId == request.AccessoryId);

            // Aksesuar üçün: mövcuddur, təsdiqlənib və stok kifayətdir
            if (request.AccessoryId is not null)
            {
                var accessory = await _db.Accessories.FirstOrDefaultAsync(
                    a => a.Id == request.AccessoryId && a.Status == ApprovalStatus.Approved,
                    cancellationToken);

                if (accessory is null)
                    throw new AppException("NOT_FOUND", StatusCodes.Status404NotFound,
                        $"Accessory (Id={request.AccessoryId}) tapılmadı.");

                var totalQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;
                if (totalQuantity > accessory.StockQty)
                    throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                        $"Stokda yalnız {accessory.StockQty} ədəd var.");
            }

            if (cart is null)
            {
                cart = new CartEntity { UserId = request.UserId };
                _db.Carts.Add(cart);
            }

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
                    AccessoryId = request.AccessoryId,
                    Quantity = request.Quantity
                });
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public record AddToCartBody(int? MotorcycleId, int? PartId, int? AccessoryId, int Quantity);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cart/items", async (AddToCartBody body, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.GetUserId();
            await sender.Send(new AddToCartCommand(userId, body.MotorcycleId, body.PartId, body.AccessoryId, body.Quantity));
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("AddToCart")
        .WithTags("Cart");
    }

    public class Validator : AbstractValidator<AddToCartCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x)
                .Must(x => new[] { x.MotorcycleId, x.PartId, x.AccessoryId }.Count(i => i.HasValue) == 1)
                .WithMessage("Ya MotorcycleId, ya PartId, ya da AccessoryId göndərilməlidir (yalnız biri).");
        }
    }
}