using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Parts;

public static class GetAllParts
{
    public record Query : IRequest<List<PartDto>>;

    public record PartDto(int Id, string Name, string Brand, decimal Price, int StockQty,
        string? ImageUrl, int PartCategoryId, string PartCategoryName);

    public class Handler : IRequestHandler<Query, List<PartDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<PartDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT p.Id, p.Name, p.Brand, p.Price, p.StockQty, p.ImageUrl,
                       p.PartCategoryId, c.Name AS PartCategoryName
                FROM Parts p
                INNER JOIN PartCategories c ON c.Id = p.PartCategoryId
                WHERE p.Status = 'Approved'
                ORDER BY p.Id DESC
                """;

            var result = await connection.QueryAsync<PartDto>(sql);
            return result.ToList();
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/parts", async (ISender sender) =>
            Results.Ok(await sender.Send(new Query())))
            .WithName("GetAllParts")
            .WithTags("Parts");
    }
}