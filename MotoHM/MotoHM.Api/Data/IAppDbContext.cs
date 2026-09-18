using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Data;
public interface IAppDbContext
{
    DbSet<MotorcycleEntity> Motorcycles { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
