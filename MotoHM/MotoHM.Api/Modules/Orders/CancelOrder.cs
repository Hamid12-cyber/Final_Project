using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Exceptions.Common;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Orders;
public static class CancelOrder
{
    public record Command(int Id, int UserId) : IRequest;

    public class Handler : IRequestHandler<Command>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var order = await _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Part)
                .Include(o => o.Items).ThenInclude(i => i.Accessory)
                .FirstOrDefaultAsync(o => o.Id == request.Id && o.UserId == request.UserId, cancellationToken);

            if (order is null)
                throw new AppException("NOT_FOUND", StatusCodes.Status404NotFound,
                    $"Order (Id={request.Id}) tapılmadı.");

            if (order.Status != OrderStatus.Pending)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Yalnız gözləmədə olan sifariş ləğv edilə bilər. Təsdiqlənmiş sifariş üçün bizimlə əlaqə saxlayın.");

            OrderRules.RestoreStock(order);

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/orders/{id:int}/cancel", async (int id, ClaimsPrincipal user, ISender sender) =>
        {
            await sender.Send(new Command(id, user.GetUserId()));
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("CancelOrder")
        .WithTags("Orders");
    }
}