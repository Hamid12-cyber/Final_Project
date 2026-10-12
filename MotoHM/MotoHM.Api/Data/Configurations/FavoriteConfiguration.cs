using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Data.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<FavoriteEntity>
{
    public void Configure(EntityTypeBuilder<FavoriteEntity> builder)
    {
        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Motorcycle)
            .WithMany()
            .HasForeignKey(f => f.MotorcycleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Hər istifadəçi hər motosikleti yalnız bir dəfə favoriyə əlavə edə bilər
        builder.HasIndex(f => new { f.UserId, f.MotorcycleId }).IsUnique();
    }
}