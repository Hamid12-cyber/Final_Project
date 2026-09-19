using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Data.Configurations;

public class PartConfiguration : IEntityTypeConfiguration<PartEntity>
{
    public void Configure(EntityTypeBuilder<PartEntity> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Brand).HasMaxLength(80).IsRequired();
        builder.Property(p => p.Price).HasColumnType("decimal(10,2)");

        builder.HasOne(p => p.PartCategory)
            .WithMany()
            .HasForeignKey(p => p.PartCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(p => p.Seller)
            .WithMany()
            .HasForeignKey(p => p.SellerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
