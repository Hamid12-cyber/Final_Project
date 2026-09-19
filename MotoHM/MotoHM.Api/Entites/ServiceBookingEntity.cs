using MotoHM.Api.Entites.Enums;

namespace MotoHM.Api.Entites;

public class ServiceBookingEntity : BaseEntity
{
    public ServiceType Type { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public int MotorcycleId { get; set; }
    public MotorcycleEntity Motorcycle { get; set; } = null!;

}