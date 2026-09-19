namespace MotoHM.Api.Entites;

public class CartItemEntity : BaseEntity
{
    public int CartId { get; set; }
    public CartEntity Cart { get; set; } = null!;

    public int? MotorcycleId { get; set; }
    public MotorcycleEntity? Motorcycle { get; set; }

    public int? PartId { get; set; }
    public PartEntity? Part { get; set; }

    public int Quantity { get; set; }
}