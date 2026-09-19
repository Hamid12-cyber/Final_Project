using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Parts;

public static class GetPartById
{
    public record Query(int Id) : IRequest<PartDetailDto?>;

    public record PartDetailDto(int Id, string Name, string Brand, decimal Price, int StockQty,
        string? ImageUrl, int PartCategoryId, string PartCategoryName);

    public class Handler : IRequestHandler<Query, PartDetailDto?>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<PartDetailDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT p.Id, p.Name, p.Brand, p.Price, p.StockQty, p.ImageUrl,
                       p.PartCategoryId, c.Name AS PartCategoryName
                FROM Parts p
                INNER JOIN PartCategories c ON c.Id = p.PartCategoryId
                WHERE p.Id = @Id
                """;

            return await connection.QueryFirstOrDefaultAsync<PartDetailDto>(sql, new { request.Id });
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/parts/{id:int}", async (int id, ISender sender) =>
        {
            var result = await sender.Send(new Query(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetPartById")
        .WithTags("Parts");
    }
}