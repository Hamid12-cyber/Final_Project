namespace MotoHM.Api.Entites;

public class FavoriteEntity : BaseEntity
{
    public int UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public int MotorcycleId { get; set; }
    public MotorcycleEntity Motorcycle { get; set; } = null!;
}
