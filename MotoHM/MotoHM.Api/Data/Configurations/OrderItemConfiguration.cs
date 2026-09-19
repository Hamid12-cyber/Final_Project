using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItemEntity>
{
    public void Configure(EntityTypeBuilder<OrderItemEntity> builder)
    {
        builder.Property(oi => oi.UnitPriceAtOrderTime).HasColumnType("decimal(10,2)");

        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.Motorcycle)
            .WithMany()
            .HasForeignKey(oi => oi.MotorcycleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(oi => oi.Part)
            .WithMany()
            .HasForeignKey(oi => oi.PartId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}