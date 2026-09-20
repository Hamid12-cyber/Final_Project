using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Data.Interceptors;

public class BackupWriteInterceptor : SaveChangesInterceptor
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackupWriteInterceptor> _logger;

    private List<(EntityState State, object Entity, Type EntityType)>? _pendingChanges;

    public BackupWriteInterceptor(IServiceScopeFactory scopeFactory, ILogger<BackupWriteInterceptor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // MSSQL-ə yazılmadan ƏVVƏL — hansı entity-lərin dəyişdiyini yadda saxlayırıq
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is AppDbContext context)
        {
            _pendingChanges = context.ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity &&
                    (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .Select(e => (e.State, e.Entity, e.Entity.GetType()))
                .ToList();
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // MSSQL-ə UĞURLA yazıldıqdan SONRA — Postgres-ə köçürürük
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (_pendingChanges is { Count: > 0 })
        {
            var changes = _pendingChanges;
            _pendingChanges = null;
            await ReplicateToBackupAsync(changes, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task ReplicateToBackupAsync(
        List<(EntityState State, object Entity, Type EntityType)> changes, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var backupDb = scope.ServiceProvider.GetRequiredService<BackupDbContext>();

            foreach (var (state, entity, entityType) in changes)
            {
                var idProperty = entityType.GetProperty("Id");
                var idValue = idProperty?.GetValue(entity);
                if (idValue is null) continue;

                var dbSet = backupDb.GetType()
                    .GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!
                    .MakeGenericMethod(entityType)
                    .Invoke(backupDb, null)!;

                var existing = await FindExistingAsync(dbSet, entityType, idValue);

                if (state == EntityState.Deleted)
                {
                    if (existing is not null)
                        backupDb.Entry(existing).State = EntityState.Deleted;
                    continue;
                }

                if (existing is null)
                {
                    var clone = Activator.CreateInstance(entityType)!;
                    CopyScalarProperties(source: entity, target: clone, entityType);
                    dbSet.GetType().GetMethod("Add")!.Invoke(dbSet, new[] { clone });
                }
                else
                {
                    CopyScalarProperties(source: entity, target: existing, entityType);
                }
            }

            await backupDb.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Postgres-ə yazma uğursuz olsa belə, MSSQL-ə (əsas axına) HEÇ BİR təsiri yoxdur —
            // yalnız log-a yazılır, istifadəçi bunu hiss etmir.
            _logger.LogWarning(ex, "Postgres backup yazması uğursuz oldu.");
        }
    }

    private static async Task<object?> FindExistingAsync(object dbSet, Type entityType, object idValue)
    {
        var findAsync = dbSet.GetType().GetMethods()
            .First(m => m.Name == "FindAsync" && m.GetParameters()[0].ParameterType == typeof(object[]));

        var task = (Task)findAsync.Invoke(dbSet, new object[] { new[] { idValue } })!;
        await task;
        return task.GetType().GetProperty("Result")!.GetValue(task);
    }

    private static void CopyScalarProperties(object source, object target, Type entityType)
    {
        foreach (var prop in entityType.GetProperties())
        {
            if (!prop.CanRead || !prop.CanWrite) continue;

            var t = prop.PropertyType;
            var isScalar = t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal)
                || t == typeof(DateTime) || t == typeof(bool) || Nullable.GetUnderlyingType(t) != null;

            if (!isScalar) continue;

            prop.SetValue(target, prop.GetValue(source));
        }
    }
}