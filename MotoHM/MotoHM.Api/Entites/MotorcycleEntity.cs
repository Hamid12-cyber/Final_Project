namespace MotoHM.Api.Entites;

public class MotorcycleEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Cc { get; set; }
    public int Year { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsForRent { get; set; }
    public bool IsForSale { get; set; } = true;
}