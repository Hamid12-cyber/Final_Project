using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItemEntity>
{
    public void Configure(EntityTypeBuilder<CartItemEntity> builder)
    {
        builder.HasOne(ci => ci.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.Motorcycle)
            .WithMany()
            .HasForeignKey(ci => ci.MotorcycleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ci => ci.Part)
            .WithMany()
            .HasForeignKey(ci => ci.PartId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}