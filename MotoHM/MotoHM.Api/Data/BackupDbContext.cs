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

            // MSSQL-dən gələn DateTime dəyərlərinin Kind-i "Unspecified"-dır (nə UTC,
            // nə local işarələnib). Npgsql defolt olaraq DateTime sütunlarını
            // "timestamp with time zone" kimi yaradır və bu tip YALNIZ Kind=Utc qəbul edir —
            // Kind=Unspecified görəndə "only UTC is supported" xətası atır.
            // "timestamp without time zone" isə Kind yoxlamadan hər DateTime-ı qəbul edir.
            foreach (var property in entityType.GetProperties())
            {
                var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                if (clrType == typeof(DateTime))
                    property.SetColumnType("timestamp without time zone");
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}