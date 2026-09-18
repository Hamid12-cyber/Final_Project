using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Entites;
using System.Reflection;

namespace MotoHM.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<MotorcycleEntity> Motorcycles => Set<MotorcycleEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}