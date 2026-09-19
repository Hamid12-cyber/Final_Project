using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Data;
public interface IAppDbContext
{
    DbSet<MotorcycleEntity> Motorcycles { get; }
    DbSet<PartCategoryEntity> PartCategories { get; }
    DbSet<PartEntity> Parts { get; }
    DbSet<TestimonialEntity> Testimonials { get; }  
    DbSet<RentalEntity> Rentals { get; }
    DbSet<ServiceBookingEntity> ServiceBookings { get; }
    DbSet<UserEntity> Users { get; }
    DbSet<CartEntity> Carts { get; }
    DbSet<CartItemEntity> CartItems { get; }
    DbSet<OrderEntity> Orders { get; }
    DbSet<OrderItemEntity> OrderItems { get; }
    DbSet<AccessoryEntity> Accessories { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
