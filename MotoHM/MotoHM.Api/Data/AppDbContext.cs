using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Entites;
using System.Reflection;

namespace MotoHM.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<MotorcycleEntity> Motorcycles => Set<MotorcycleEntity>();
    public DbSet<PartCategoryEntity> PartCategories => Set<PartCategoryEntity>();
    public DbSet<PartEntity> Parts => Set<PartEntity>();
    public DbSet<TestimonialEntity> Testimonials => Set<TestimonialEntity>();
    public DbSet<RentalEntity> Rentals => Set<RentalEntity>();
    public DbSet<ServiceBookingEntity> ServiceBookings => Set<ServiceBookingEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();

    public DbSet<CartEntity> Carts => Set<CartEntity>();
    public DbSet<CartItemEntity> CartItems => Set<CartItemEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();
    public DbSet<AccessoryEntity> Accessories => Set<AccessoryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<MotorcycleEntity>().HasQueryFilter(m => !m.IsDeleted);
        modelBuilder.Entity<PartEntity>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<AccessoryEntity>().HasQueryFilter(a => !a.IsDeleted);
        modelBuilder.Entity<UserEntity>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<OrderEntity>().HasQueryFilter(o => !o.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }
}