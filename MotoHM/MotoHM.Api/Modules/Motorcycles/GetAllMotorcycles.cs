using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Motorcycles;

public static class GetAllMotorcycles
{
    public record Query : IRequest<List<MotorcycleDto>>;

    public record MotorcycleDto(int Id, string Name, string Brand, string Model, int Cc, int Year, decimal Price, string? ImageUrl, bool IsForRent, bool IsForSale);

    public class Handler : IRequestHandler<Query, List<MotorcycleDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<MotorcycleDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT Id, Name, Brand, Model, Cc, Year, Price, ImageUrl, IsForRent, IsForSale
                FROM Motorcycles
                WHERE Status = 'Approved' AND IsDeleted = 0
                ORDER BY Id DESC
                """;

            var result = await connection.QueryAsync<MotorcycleDto>(sql);
            return result.ToList();
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/motorcycles", async (ISender sender) =>
            Results.Ok(await sender.Send(new Query())))
            .WithName("GetAllMotorcycles")
            .WithTags("Motorcycles");
    }
}