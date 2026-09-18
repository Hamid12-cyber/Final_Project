using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Data.Configurations;

public class MotorcycleConfiguration : IEntityTypeConfiguration<MotorcycleEntity>
{
    public void Configure(EntityTypeBuilder<MotorcycleEntity> builder)
    {
        builder.Property(m => m.Price).HasColumnType("decimal(10,2)");
        builder.Property(m => m.Name).HasMaxLength(150).IsRequired();
        builder.Property(m => m.Brand).HasMaxLength(80).IsRequired();
        builder.Property(m => m.Model).HasMaxLength(80).IsRequired();
    }
}