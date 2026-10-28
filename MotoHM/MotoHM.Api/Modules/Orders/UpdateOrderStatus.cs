using MotoHM.Api.Entites.Enums;

namespace MotoHM.Api.Modules.Orders;

public static class UpdateOrderStatus
{
    public record Command(int Id, OrderStatus Status) : IRequest;

    public class Handler : IRequestHandler<Command>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var order = await _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Part)
                .Include(o => o.Items).ThenInclude(i => i.Accessory)
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            if (order is null)
                throw new AppException("NOT_FOUND", StatusCodes.Status404NotFound,
                    $"Order (Id={request.Id}) tapılmadı.");

            if (!OrderRules.CanTransition(order.Status, request.Status))
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    $"Sifarişin statusunu '{order.Status}' vəziyyətindən '{request.Status}' vəziyyətinə keçirmək olmaz.");

            if (request.Status == OrderStatus.Cancelled)
                OrderRules.RestoreStock(order);

            order.Status = request.Status;
            order.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public record UpdateStatusBody(OrderStatus Status);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Status).IsInEnum().WithMessage("Yanlış status.");
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/admin/orders/{id:int}/status", async (int id, UpdateStatusBody body, ISender sender) =>
        {
            await sender.Send(new Command(id, body.Status));
            return Results.NoContent();
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("UpdateOrderStatus")
        .WithTags("Admin");
    }
}