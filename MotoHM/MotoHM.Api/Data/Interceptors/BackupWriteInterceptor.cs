using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Data.Interceptors;

public class BackupWriteInterceptor : SaveChangesInterceptor
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackupWriteInterceptor> _logger;

    // BU İNTERCEPTOR SINGLETON-DUR — bütün paralel request-lər arasında EYNİ obyekt
    // paylaşılır. Ona görə "_pendingChanges" kimi adi bir instance sahəsi İŞLƏMİR:
    // iki sorğu eyni anda gələndə bir-birinin üzərinə yazır (race condition).
    //
    // Həll: dəyişiklikləri instance sahəsində deyil, HƏR DbContext OBYEKTİNƏ görə
    // ayrıca saxlayırıq. ConditionalWeakTable dəqiq bunun üçündür — açar (context)
    // "dispose" olunanda GC tərəfindən avtomatik təmizlənir, əlavə heç nə etməyə ehtiyac yoxdur.
    private static readonly ConditionalWeakTable<DbContext, List<PendingChange>> _pending = new();

    private sealed record PendingChange(EntityState State, object Entity, Type EntityType);

    public BackupWriteInterceptor(IServiceScopeFactory scopeFactory, ILogger<BackupWriteInterceptor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is AppDbContext context)
        {
            var changes = context.ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity &&
                    (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .Select(e => new PendingChange(e.State, e.Entity, e.Entity.GetType()))
                .ToList();

            // Konkret bu context instansı üçün saxlanılır — başqa paralel request-in
            // öz AppDbContext instansına heç bir təsiri yoxdur.
            _pending.AddOrUpdate(context, changes);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is AppDbContext context &&
            _pending.TryGetValue(context, out var changes))
        {
            _pending.Remove(context);

            if (changes.Count > 0)
                await ReplicateToBackupAsync(changes);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    // Diqqət: heç bir parametr almır — request-in cancellationToken-indən TAM asılı deyil.
    // Backup əməliyyatı HTTP cavabı göndəriləndən sonra da tam bitməlidir.
    private async Task ReplicateToBackupAsync(List<PendingChange> changes)
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

                var existing = await FindExistingAsync(dbSet, idValue);

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

            await backupDb.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Postgres-ə yazma uğursuz olsa belə, MSSQL-ə (əsas axına) HEÇ BİR təsiri yoxdur —
            // yalnız log-a yazılır, istifadəçi bunu hiss etmir.
            _logger.LogWarning(ex, "Postgres backup yazması uğursuz oldu.");
        }
    }

    private static async Task<object?> FindExistingAsync(object dbSet, object idValue)
    {
        // DbSet<T>.FindAsync(object[]) Task yox, ValueTask<T> (struct) qaytarır.
        // ValueTask birbaşa Task-a cast oluna bilmir — .AsTask() ilə Task-a çeviririk,
        // beləliklə reflection nəticəsini adi "await task" ilə gözləyə bilirik.
        var findAsync = dbSet.GetType().GetMethods()
            .First(m => m.Name == "FindAsync" && m.GetParameters().Length == 1
                && m.GetParameters()[0].ParameterType == typeof(object[]));

        var valueTaskResult = findAsync.Invoke(dbSet, new object[] { new[] { idValue } })!;
        var asTaskMethod = valueTaskResult.GetType().GetMethod("AsTask")!;
        var task = (Task)asTaskMethod.Invoke(valueTaskResult, null)!;

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