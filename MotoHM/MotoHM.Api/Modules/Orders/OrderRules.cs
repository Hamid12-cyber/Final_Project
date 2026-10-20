using MotoHM.Api.Entites;
using MotoHM.Api.Entites.Enums;

namespace MotoHM.Api.Modules.Orders;

public static class OrderRules
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> Transitions = new()
    {
        [OrderStatus.Pending] = new[] { OrderStatus.Confirmed, OrderStatus.Cancelled },
        [OrderStatus.Confirmed] = new[] { OrderStatus.Shipped, OrderStatus.Cancelled },
        [OrderStatus.Shipped] = new[] { OrderStatus.Delivered },
        [OrderStatus.Delivered] = Array.Empty<OrderStatus>(),
        [OrderStatus.Cancelled] = Array.Empty<OrderStatus>(),
    };

    public static bool CanTransition(OrderStatus from, OrderStatus to) =>
        Transitions.TryGetValue(from, out var next) && next.Contains(to);
    public static void RestoreStock(OrderEntity order)
    {
        foreach (var item in order.Items)
        {
            if (item.Part is not null)
                item.Part.StockQty += item.Quantity;

            if (item.Accessory is not null)
                item.Accessory.StockQty += item.Quantity;
        }
    }
}