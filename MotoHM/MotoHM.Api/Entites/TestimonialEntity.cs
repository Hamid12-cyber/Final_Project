namespace MotoHM.Api.Entites;

public class TestimonialEntity : BaseEntity
{
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
}