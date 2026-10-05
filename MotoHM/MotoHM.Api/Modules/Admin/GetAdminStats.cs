using Dapper;
using MediatR;
using MotoHM.Api.Data;
using MotoHM.Api.Entites.Enums;

namespace MotoHM.Api.Modules.Admin;

public static class GetAdminStats
{
    public record Query : IRequest<Response>;

    public record StatusCount(string Status, int Count);

    public record DailyStat(string Date, int Orders, decimal Revenue);

    public record TopProduct(string Type, int Id, string Name, int SoldQty, decimal Revenue);

    public record Response(
        decimal TotalRevenue,
        decimal RevenueLast30Days,
        int TotalOrders,
        int PendingOrders,
        int TotalUsers,
        int PendingListings,
        List<StatusCount> OrdersByStatus,
        List<DailyStat> Last30Days,
        List<TopProduct> TopProducts);

    public record OrderCounts(int TotalOrders, int PendingOrders);

    public record DailyRow(DateTime Day, int Orders, decimal Revenue);

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            var from = DateTime.UtcNow.Date.AddDays(-29);

            // Ləğv olunmuş sifarişlər gəlirə və satışa daxil edilmir
            const string sql = """
                SELECT COALESCE(SUM(TotalAmount), 0)
                FROM Orders
                WHERE IsDeleted = 0 AND Status <> 'Cancelled';

                SELECT COUNT(*) AS TotalOrders,
                       COALESCE(SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END), 0) AS PendingOrders
                FROM Orders
                WHERE IsDeleted = 0;

                SELECT COUNT(*) FROM Users WHERE IsDeleted = 0;

                SELECT (SELECT COUNT(*) FROM Motorcycles WHERE IsDeleted = 0 AND Status = 'Pending')
                     + (SELECT COUNT(*) FROM Parts WHERE IsDeleted = 0 AND Status = 'Pending')
                     + (SELECT COUNT(*) FROM Accessories WHERE IsDeleted = 0 AND Status = 'Pending');

                SELECT Status, COUNT(*) AS Count
                FROM Orders
                WHERE IsDeleted = 0
                GROUP BY Status;

                SELECT CAST(CreatedAt AS date) AS Day, COUNT(*) AS Orders, COALESCE(SUM(TotalAmount), 0) AS Revenue
                FROM Orders
                WHERE IsDeleted = 0 AND Status <> 'Cancelled' AND CreatedAt >= @From
                GROUP BY CAST(CreatedAt AS date);

                SELECT TOP 5 Type, Id, Name, SUM(Quantity) AS SoldQty, SUM(Quantity * UnitPriceAtOrderTime) AS Revenue
                FROM (
                    SELECT 'motorcycle' AS Type, m.Id, m.Name, oi.Quantity, oi.UnitPriceAtOrderTime
                    FROM OrderItems oi
                    INNER JOIN Orders o ON o.Id = oi.OrderId
                    INNER JOIN Motorcycles m ON m.Id = oi.MotorcycleId
                    WHERE o.IsDeleted = 0 AND o.Status <> 'Cancelled'
                    UNION ALL
                    SELECT 'part', p.Id, p.Name, oi.Quantity, oi.UnitPriceAtOrderTime
                    FROM OrderItems oi
                    INNER JOIN Orders o ON o.Id = oi.OrderId
                    INNER JOIN Parts p ON p.Id = oi.PartId
                    WHERE o.IsDeleted = 0 AND o.Status <> 'Cancelled'
                    UNION ALL
                    SELECT 'accessory', a.Id, a.Name, oi.Quantity, oi.UnitPriceAtOrderTime
                    FROM OrderItems oi
                    INNER JOIN Orders o ON o.Id = oi.OrderId
                    INNER JOIN Accessories a ON a.Id = oi.AccessoryId
                    WHERE o.IsDeleted = 0 AND o.Status <> 'Cancelled'
                ) AS sold
                GROUP BY Type, Id, Name
                ORDER BY SoldQty DESC, Revenue DESC;
                """;

            using var multi = await connection.QueryMultipleAsync(sql, new { From = from });

            var totalRevenue = await multi.ReadSingleAsync<decimal>();
            var counts = await multi.ReadSingleAsync<OrderCounts>();
            var totalUsers = await multi.ReadSingleAsync<int>();
            var pendingListings = await multi.ReadSingleAsync<int>();
            var statusRows = (await multi.ReadAsync<StatusCount>()).ToList();
            var dailyRows = (await multi.ReadAsync<DailyRow>()).ToList();
            var topProducts = (await multi.ReadAsync<TopProduct>()).ToList();

            // Bütün statusları göstəririk, sifarişi olmayanlar 0 ilə
            var ordersByStatus = Enum.GetValues<OrderStatus>()
                .Select(s => new StatusCount(
                    s.ToString(),
                    statusRows.FirstOrDefault(r => r.Status == s.ToString())?.Count ?? 0))
                .ToList();

            // Son 30 günün hər günü olsun, satış olmayan günlər 0 ilə
            var last30Days = Enumerable.Range(0, 30)
                .Select(i =>
                {
                    var day = from.AddDays(i);
                    var row = dailyRows.FirstOrDefault(r => r.Day.Date == day);
                    return new DailyStat(day.ToString("yyyy-MM-dd"), row?.Orders ?? 0, row?.Revenue ?? 0);
                })
                .ToList();

            return new Response(
                totalRevenue,
                last30Days.Sum(d => d.Revenue),
                counts.TotalOrders,
                counts.PendingOrders,
                totalUsers,
                pendingListings,
                ordersByStatus,
                last30Days,
                topProducts);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/stats", async (ISender sender) =>
            Results.Ok(await sender.Send(new Query())))
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("GetAdminStats")
            .WithTags("Admin");
    }
}