using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Accessories;

public static class GetAccessoryById
{
    public record Query(int Id) : IRequest<AccessoryDetailDto?>;

    public record AccessoryDetailDto(int Id, string Name, string Brand, decimal Price, int StockQty, string? ImageUrl);

    public class Handler : IRequestHandler<Query, AccessoryDetailDto?>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<AccessoryDetailDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT Id, Name, Brand, Price, StockQty, ImageUrl
                FROM Accessories
                WHERE Id = @Id AND IsDeleted = 0
                """;

            return await connection.QueryFirstOrDefaultAsync<AccessoryDetailDto>(sql, new { request.Id });
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/accessories/{id:int}", async (int id, ISender sender) =>
        {
            var result = await sender.Send(new Query(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetAccessoryById")
        .WithTags("Accessories");
    }
}