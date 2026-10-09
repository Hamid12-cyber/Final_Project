namespace MotoHM.Api.Entites;

public class ReviewEntity : BaseEntity
{
    public int UserId { get; set; }
    public UserEntity User { get; set; } = null!;
    public int? MotorcycleId { get; set; }
    public MotorcycleEntity? Motorcycle { get; set; }

    public int? PartId { get; set; }
    public PartEntity? Part { get; set; }

    public int? AccessoryId { get; set; }
    public AccessoryEntity? Accessory { get; set; }

    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
}