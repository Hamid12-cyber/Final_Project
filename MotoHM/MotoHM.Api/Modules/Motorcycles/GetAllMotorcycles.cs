using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Motorcycles;

public static class GetAllMotorcycles
{
    public record Query : IRequest<List<MotorcycleDto>>;

    public record MotorcycleDto(int Id, string Name, string Brand, string Model, int Cc, int Year, decimal Price,
        string? ImageUrl, bool IsForRent, bool IsForSale, double AverageRating, int ReviewCount);

    public class Handler : IRequestHandler<Query, List<MotorcycleDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<MotorcycleDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT m.Id, m.Name, m.Brand, m.Model, m.Cc, m.Year, m.Price, m.ImageUrl, m.IsForRent, m.IsForSale,
                       COALESCE((SELECT AVG(CAST(r.Rating AS float)) FROM Reviews r
                                 WHERE r.MotorcycleId = m.Id AND r.IsDeleted = 0), 0) AS AverageRating,
                       (SELECT COUNT(*) FROM Reviews r
                        WHERE r.MotorcycleId = m.Id AND r.IsDeleted = 0) AS ReviewCount
                FROM Motorcycles m
                WHERE m.Status = 'Approved' AND m.IsDeleted = 0
                ORDER BY m.Id DESC
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