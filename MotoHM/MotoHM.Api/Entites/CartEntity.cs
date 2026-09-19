namespace MotoHM.Api.Entites;

public class CartEntity : BaseEntity
{
    public int UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public ICollection<CartItemEntity> Items { get; set; } = new List<CartItemEntity>();
}