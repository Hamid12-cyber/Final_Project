using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Entites;
using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Exceptions.Common;
using MotoHM.Api.Shared.Extensions;
using System.Data;
using System.Security.Claims;

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
            try
            {
                // Serializable transaction: eyni anda iki nəfər eyni son stoku / eyni motosikleti sifariş etsə,
                // biri gözləyir və ya geri çəkilir -> stok mənfiyə düşmür, motosiklet iki dəfə satılmır.
                await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

                var orderId = await CreateAsync(request, cancellationToken);

                await tx.CommitAsync(cancellationToken);
                return orderId;
            }
            catch (Exception ex) when (IsDeadlock(ex))
            {
                throw new AppException("CONFLICT", StatusCodes.Status409Conflict,
                    "Eyni anda çoxlu sifariş gəldi, zəhmət olmasa bir az sonra yenidən cəhd edin.");
            }
        }

        private async Task<int> CreateAsync(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var cart = await _db.Carts
                .Include(c => c.Items).ThenInclude(i => i.Motorcycle)
                .Include(c => c.Items).ThenInclude(i => i.Part)
                .Include(c => c.Items).ThenInclude(i => i.Accessory)
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
                // Sifariş anında hər məhsul yenidən yoxlanılır: təsdiqlənib və stok kifayətdir
                if (cartItem.Motorcycle is not null)
                {
                    if (cartItem.Motorcycle.Status != ApprovalStatus.Approved)
                        throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                            $"'{cartItem.Motorcycle.Name}' motosikleti hazırda satışda deyil.");

                    // Motosiklet tək elandır: ləğv olunmamış sifarişdə varsa, artıq satılıb.
                    var motorcycleId = cartItem.MotorcycleId;
                    var alreadyOrdered = await _db.OrderItems.AnyAsync(oi =>
                        oi.MotorcycleId == motorcycleId &&
                        oi.Order.Status != OrderStatus.Cancelled, cancellationToken);

                    if (alreadyOrdered)
                        throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                            $"'{cartItem.Motorcycle.Name}' motosikleti artıq sifariş olunub.");
                }

                if (cartItem.Part is not null)
                {
                    if (cartItem.Part.Status != ApprovalStatus.Approved)
                        throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                            $"'{cartItem.Part.Name}' hissəsi hazırda satışda deyil.");

                    if (cartItem.Quantity > cartItem.Part.StockQty)
                        throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                            $"'{cartItem.Part.Name}' üçün stokda yalnız {cartItem.Part.StockQty} ədəd var.");

                    cartItem.Part.StockQty -= cartItem.Quantity;
                }

                if (cartItem.Accessory is not null)
                {
                    if (cartItem.Accessory.Status != ApprovalStatus.Approved)
                        throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                            $"'{cartItem.Accessory.Name}' aksesuarı hazırda satışda deyil.");

                    if (cartItem.Quantity > cartItem.Accessory.StockQty)
                        throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                            $"'{cartItem.Accessory.Name}' üçün stokda yalnız {cartItem.Accessory.StockQty} ədəd var.");

                    cartItem.Accessory.StockQty -= cartItem.Quantity;
                }

                var unitPrice = cartItem.Motorcycle?.Price ?? cartItem.Part?.Price ?? cartItem.Accessory?.Price
                    ?? throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                        "Səbətdəki bir məhsulun qiyməti tapılmadı.");

                order.Items.Add(new OrderItemEntity
                {
                    MotorcycleId = cartItem.MotorcycleId,
                    PartId = cartItem.PartId,
                    AccessoryId = cartItem.AccessoryId,
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

        // SQL Server deadlock (xəta nömrəsi 1205): Serializable zamanı iki sorğu toqquşanda biri "qurban" seçilir
        private static bool IsDeadlock(Exception ex)
        {
            for (var e = ex; e is not null; e = e.InnerException)
            {
                if (e is Microsoft.Data.SqlClient.SqlException { Number: 1205 })
                    return true;
            }
            return false;
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

    public class Validator : AbstractValidator<CreateOrderCommand>
    {
        public Validator()
        {
            RuleFor(x => x.ShippingAddress).NotEmpty().MaximumLength(300);
            RuleFor(x => x.ContactPhone).NotEmpty().MaximumLength(30);
        }
    }
}