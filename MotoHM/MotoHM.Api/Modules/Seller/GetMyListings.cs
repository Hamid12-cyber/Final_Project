using System.Security.Claims;
using Dapper;
using MediatR;
using MotoHM.Api.Data;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Seller;

public static class GetMyListings
{
    public record Query(int UserId) : IRequest<List<ListingDto>>;

    // Type: motorcycle | part | accessory. Redaktə formu üçün lazım olan əlavə sahələr tipə görə null ola bilər.
    public record ListingDto(string Type, int Id, string Name, string Brand, decimal Price, int? StockQty,
        string? ImageUrl, string Status, DateTime CreatedAt,
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
                       CAST(NULL AS int) AS StockQty, m.ImageUrl, m.Status, m.CreatedAt,
                       m.Model, m.Cc, m.Year, m.IsForRent, m.IsForSale, CAST(NULL AS int) AS PartCategoryId,
                       m.RejectionReason
                FROM Motorcycles m
                WHERE m.SellerId = @UserId AND m.IsDeleted = 0

                UNION ALL

                SELECT 'part', p.Id, p.Name, p.Brand, p.Price,
                       p.StockQty, p.ImageUrl, p.Status, p.CreatedAt,
                       CAST(NULL AS nvarchar(80)), CAST(NULL AS int), CAST(NULL AS int),
                       CAST(NULL AS bit), CAST(NULL AS bit), p.PartCategoryId,
                       p.RejectionReason
                FROM Parts p
                WHERE p.SellerId = @UserId AND p.IsDeleted = 0

                UNION ALL

                SELECT 'accessory', a.Id, a.Name, a.Brand, a.Price,
                       a.StockQty, a.ImageUrl, a.Status, a.CreatedAt,
                       CAST(NULL AS nvarchar(80)), CAST(NULL AS int), CAST(NULL AS int),
                       CAST(NULL AS bit), CAST(NULL AS bit), CAST(NULL AS int),
                       a.RejectionReason
                FROM Accessories a
                WHERE a.SellerId = @UserId AND a.IsDeleted = 0

                ORDER BY CreatedAt DESC
                """;

            var result = await connection.QueryAsync<ListingDto>(sql, new { request.UserId });
            return result.ToList();
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