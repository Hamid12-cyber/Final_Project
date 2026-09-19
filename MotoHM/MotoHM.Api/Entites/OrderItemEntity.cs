namespace MotoHM.Api.Entites;

public class OrderItemEntity : BaseEntity
{
    public int OrderId { get; set; }
    public OrderEntity Order { get; set; } = null!;

    public int? MotorcycleId { get; set; }
    public MotorcycleEntity? Motorcycle { get; set; }

    public int? PartId { get; set; }
    public PartEntity? Part { get; set; }

    public int Quantity { get; set; }
        
    // məhsulun qiyməti dəyişsə belə, keçmiş sifarişin qiyməti dəyişməməlidir
    public decimal UnitPriceAtOrderTime { get; set; }
}