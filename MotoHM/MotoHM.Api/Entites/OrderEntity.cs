using MotoHM.Api.Entites.Enums;

namespace MotoHM.Api.Entites;

public class OrderEntity : BaseEntity
{
    public int UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;

    public ICollection<OrderItemEntity> Items { get; set; } = new List<OrderItemEntity>();
}