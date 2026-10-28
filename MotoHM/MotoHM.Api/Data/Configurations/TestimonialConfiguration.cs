
namespace MotoHM.Api.Data.Configurations;

public class TestimonialConfiguration : IEntityTypeConfiguration<TestimonialEntity>
{
    public void Configure(EntityTypeBuilder<TestimonialEntity> builder)
    {
        builder.Property(t => t.CustomerName).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Text).HasMaxLength(1000).IsRequired();
    }
}
