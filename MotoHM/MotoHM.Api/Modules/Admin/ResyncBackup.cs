using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Modules.Admin;

/// <summary>
/// Backup (PostgreSQL) bazasını sıfırdan MSSQL-dən yenidən qurur.
/// Lazımdır çünki: 1) backup inteqrasiyasından ƏVVƏL yaranmış data heç vaxt
/// avtomatik köçürülməyib, 2) dual-write ara vaxtlarda uğursuz olub (log-a bax).
/// FK sırası ilə: əvvəlcə valideyn cədvəllər (Users), sonra ona bağlı olanlar.
/// </summary>
public static class ResyncBackup
{
    public record Command : IRequest<Dictionary<string, int>>;

    public class Handler : IRequestHandler<Command, Dictionary<string, int>>
    {
        private readonly AppDbContext _primary;
        private readonly BackupDbContext _backup;
        private readonly ILogger<Handler> _logger;

        public Handler(AppDbContext primary, BackupDbContext backup, ILogger<Handler> logger)
        {
            _primary = primary;
            _backup = backup;
            _logger = logger;
        }

        public async Task<Dictionary<string, int>> Handle(Command request, CancellationToken ct)
        {
            var report = new Dictionary<string, int>();

            // Silmə: uşaqdan valideynə doğru (FK pozulmasın)
            await _backup.OrderItems.ExecuteDeleteAsync(ct);
            await _backup.Orders.ExecuteDeleteAsync(ct);
            await _backup.ServiceBookings.ExecuteDeleteAsync(ct);
            await _backup.Rentals.ExecuteDeleteAsync(ct);
            await _backup.Testimonials.ExecuteDeleteAsync(ct);
            await _backup.Accessories.ExecuteDeleteAsync(ct);
            await _backup.Parts.ExecuteDeleteAsync(ct);
            await _backup.PartCategories.ExecuteDeleteAsync(ct);
            await _backup.Motorcycles.ExecuteDeleteAsync(ct);
            await _backup.Users.ExecuteDeleteAsync(ct);

            // Köçürmə: valideyndən uşağa doğru
            report["Users"] = await CopyAsync(_primary.Users, _backup.Users, ct);
            report["Motorcycles"] = await CopyAsync(_primary.Motorcycles, _backup.Motorcycles, ct);
            report["PartCategories"] = await CopyAsync(_primary.PartCategories, _backup.PartCategories, ct);
            report["Parts"] = await CopyAsync(_primary.Parts, _backup.Parts, ct);
            report["Accessories"] = await CopyAsync(_primary.Accessories, _backup.Accessories, ct);
            report["Testimonials"] = await CopyAsync(_primary.Testimonials, _backup.Testimonials, ct);
            report["Rentals"] = await CopyAsync(_primary.Rentals, _backup.Rentals, ct);
            report["ServiceBookings"] = await CopyAsync(_primary.ServiceBookings, _backup.ServiceBookings, ct);
            report["Orders"] = await CopyAsync(_primary.Orders, _backup.Orders, ct);
            report["OrderItems"] = await CopyAsync(_primary.OrderItems, _backup.OrderItems, ct);

            _logger.LogInformation("Backup resync tamamlandı: {Total} sətir.", report.Values.Sum());
            return report;
        }

        private async Task<int> CopyAsync<T>(DbSet<T> source, DbSet<T> target, CancellationToken ct)
            where T : class
        {
            var rows = await source.AsNoTracking().ToListAsync(ct);
            if (rows.Count == 0) return 0;

            await target.AddRangeAsync(rows, ct);
            await _backup.SaveChangesAsync(ct);
            _backup.ChangeTracker.Clear();

            return rows.Count;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/backup/resync", async (ISender sender) =>
            Results.Ok(await sender.Send(new Command())))
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("ResyncBackup")
            .WithTags("Admin");
    }
}