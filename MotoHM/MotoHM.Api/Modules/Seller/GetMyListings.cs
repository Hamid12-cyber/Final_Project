using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Seller;

public static class GetMyListings
{
    public record Query(int UserId) : IRequest<List<ListingDto>>;
    public record ListingDto(string Type, int Id, string Name, string Brand, decimal Price, int? StockQty,
        string? ImageUrl, IReadOnlyList<string> ImageUrls, string Status, DateTime CreatedAt,
        string? Model, int? Cc, int? Year, bool? IsForRent, bool? IsForSale, int? PartCategoryId,
        string? RejectionReason);

    // SQL nəticəsini oxumaq üçün (ImageUrlsJson burada JSON mətn kimi gəlir)
    public record ListingRow(string Type, int Id, string Name, string Brand, decimal Price, int? StockQty,
        string? ImageUrl, string? ImageUrlsJson, string Status, DateTime CreatedAt,
        string? Model, int? Cc, int? Year, bool? IsForRent, bool? IsForSale, int? PartCategoryId,
        string? RejectionReason);

    public class Handler : IRequestHandler<Query, List<ListingDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<ListingDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT 'motorcycle' AS Type, m.Id, m.Name, m.Brand, m.Price,
                       CAST(NULL AS int) AS StockQty, m.ImageUrl, m.ImageUrlsJson, m.Status, m.CreatedAt,
                       m.Model, m.Cc, m.Year, m.IsForRent, m.IsForSale, CAST(NULL AS int) AS PartCategoryId,
                       m.RejectionReason
                FROM Motorcycles m
                WHERE m.SellerId = @UserId AND m.IsDeleted = 0

                UNION ALL

                SELECT 'part', p.Id, p.Name, p.Brand, p.Price,
                       p.StockQty, p.ImageUrl, p.ImageUrlsJson, p.Status, p.CreatedAt,
                       CAST(NULL AS nvarchar(80)), CAST(NULL AS int), CAST(NULL AS int),
                       CAST(NULL AS bit), CAST(NULL AS bit), p.PartCategoryId,
                       p.RejectionReason
                FROM Parts p
                WHERE p.SellerId = @UserId AND p.IsDeleted = 0

                UNION ALL

                SELECT 'accessory', a.Id, a.Name, a.Brand, a.Price,
                       a.StockQty, a.ImageUrl, a.ImageUrlsJson, a.Status, a.CreatedAt,
                       CAST(NULL AS nvarchar(80)), CAST(NULL AS int), CAST(NULL AS int),
                       CAST(NULL AS bit), CAST(NULL AS bit), CAST(NULL AS int),
                       a.RejectionReason
                FROM Accessories a
                WHERE a.SellerId = @UserId AND a.IsDeleted = 0

                ORDER BY CreatedAt DESC
                """;

            var rows = await connection.QueryAsync<ListingRow>(sql, new { request.UserId });

            return rows.Select(r => new ListingDto(r.Type, r.Id, r.Name, r.Brand, r.Price, r.StockQty,
                r.ImageUrl, ImageList.FromJson(r.ImageUrlsJson, r.ImageUrl), r.Status, r.CreatedAt,
                r.Model, r.Cc, r.Year, r.IsForRent, r.IsForSale, r.PartCategoryId, r.RejectionReason)).ToList();
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/seller/listings", async (ClaimsPrincipal user, ISender sender) =>
            Results.Ok(await sender.Send(new Query(user.GetUserId()))))
            .RequireAuthorization()
            .WithName("GetMyListings")
            .WithTags("Seller");
    }
}