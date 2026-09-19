using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Data.Configurations;

public class ServiceBookingConfiguration : IEntityTypeConfiguration<ServiceBookingEntity>
{
    public void Configure(EntityTypeBuilder<ServiceBookingEntity> builder)
    {
        builder.Property(s => s.CustomerName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.CustomerPhone).HasMaxLength(30).IsRequired();
        builder.Property(s => s.Notes).HasMaxLength(500);
        builder.Property(s => s.Type).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(s => s.Motorcycle)
            .WithMany()
            .HasForeignKey(s => s.MotorcycleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}