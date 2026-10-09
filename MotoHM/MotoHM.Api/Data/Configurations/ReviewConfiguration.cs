using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<ReviewEntity>
{
    public void Configure(EntityTypeBuilder<ReviewEntity> builder)
    {
        builder.Property(r => r.Comment).HasMaxLength(1000);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Motorcycle)
            .WithMany()
            .HasForeignKey(r => r.MotorcycleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Part)
            .WithMany()
            .HasForeignKey(r => r.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Accessory)
            .WithMany()
            .HasForeignKey(r => r.AccessoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Hər istifadəçi hər məhsula yalnız bir rəy yaza bilər
        builder.HasIndex(r => new { r.UserId, r.MotorcycleId })
            .IsUnique()
            .HasFilter("[MotorcycleId] IS NOT NULL");

        builder.HasIndex(r => new { r.UserId, r.PartId })
            .IsUnique()
            .HasFilter("[PartId] IS NOT NULL");

        builder.HasIndex(r => new { r.UserId, r.AccessoryId })
            .IsUnique()
            .HasFilter("[AccessoryId] IS NOT NULL");
    }
}