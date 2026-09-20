using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Data;

public class BackupDbContext(DbContextOptions<BackupDbContext> options) : DbContext(options)
{
    public DbSet<MotorcycleEntity> Motorcycles => Set<MotorcycleEntity>();
    public DbSet<PartCategoryEntity> PartCategories => Set<PartCategoryEntity>();
    public DbSet<PartEntity> Parts => Set<PartEntity>();
    public DbSet<TestimonialEntity> Testimonials => Set<TestimonialEntity>();
    public DbSet<RentalEntity> Rentals => Set<RentalEntity>();
    public DbSet<ServiceBookingEntity> ServiceBookings => Set<ServiceBookingEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<AccessoryEntity> Accessories => Set<AccessoryEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var idProperty = entityType.FindProperty("Id");
            if (idProperty is not null)
            {
                idProperty.ValueGenerated = ValueGenerated.Never;
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}