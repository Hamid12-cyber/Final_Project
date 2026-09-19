using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Data.Configurations;

public class AccessoryConfiguration : IEntityTypeConfiguration<AccessoryEntity>
{
    public void Configure(EntityTypeBuilder<AccessoryEntity> builder)
    {
        builder.Property(a => a.Name).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Brand).HasMaxLength(80).IsRequired();
        builder.Property(a => a.Price).HasColumnType("decimal(10,2)");
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(a => a.Seller)
            .WithMany()
            .HasForeignKey(a => a.SellerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}