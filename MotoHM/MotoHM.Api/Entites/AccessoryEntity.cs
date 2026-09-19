using MotoHM.Api.Entites.Enums;

namespace MotoHM.Api.Entites;

public class AccessoryEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQty { get; set; }
    public string? ImageUrl { get; set; }

    public int SellerId { get; set; }
    public UserEntity Seller { get; set; } = null!;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

}