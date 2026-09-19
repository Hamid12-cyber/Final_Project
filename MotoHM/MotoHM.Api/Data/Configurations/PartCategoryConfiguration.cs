using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Data.Configurations;

public class PartCategoryConfiguration : IEntityTypeConfiguration<PartCategoryEntity>
{
    public void Configure(EntityTypeBuilder<PartCategoryEntity> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Icon).HasMaxLength(20).IsRequired();
    }
}