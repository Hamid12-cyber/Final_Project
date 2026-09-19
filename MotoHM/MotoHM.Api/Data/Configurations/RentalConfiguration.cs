using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoHM.Api.Entites;
using MotoHM.Api.Entites.Enums;

namespace MotoHM.Api.Data.Configurations;

public class RentalConfiguration : IEntityTypeConfiguration<RentalEntity>
{
    public void Configure(EntityTypeBuilder<RentalEntity> builder)
    {
        builder.Property(r => r.TotalPrice).HasColumnType("decimal(10,2)");
        builder.Property(r => r.CustomerName).HasMaxLength(100).IsRequired();
        builder.Property(r => r.CustomerPhone).HasMaxLength(30).IsRequired();
        builder.Property(r => r.Period).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(r => r.Motorcycle)
            .WithMany()
            .HasForeignKey(r => r.MotorcycleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}