using MotoHM.Api.Entites.Enums;

namespace MotoHM.Api.Entites;

public class RentalEntity : BaseEntity
{
    public int MotorcycleId { get; set; }
    public MotorcycleEntity Motorcycle { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public RentalPeriod Period { get; set; }
    public decimal TotalPrice { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
}