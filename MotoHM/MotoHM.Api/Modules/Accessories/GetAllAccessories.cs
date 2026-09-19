using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Accessories;

public static class GetAllAccessories
{
    public record Query : IRequest<List<AccessoryDto>>;

    public record AccessoryDto(int Id, string Name, string Brand, decimal Price, int StockQty, string? ImageUrl);

    public class Handler : IRequestHandler<Query, List<AccessoryDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<AccessoryDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT Id, Name, Brand, Price, StockQty, ImageUrl
                FROM Accessories
                WHERE Status = 'Approved'
                ORDER BY Id DESC
                """;

            var result = await connection.QueryAsync<AccessoryDto>(sql);
            return result.ToList();
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/accessories", async (ISender sender) =>
            Results.Ok(await sender.Send(new Query())))
            .WithName("GetAllAccessories")
            .WithTags("Accessories");
    }
}