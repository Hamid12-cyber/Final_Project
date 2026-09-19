using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Admin;

public static class GetPendingMotorcycles
{
    public record Query : IRequest<List<PendingMotorcycleDto>>;

    public record PendingMotorcycleDto(int Id, string Name, string Brand, decimal Price, int SellerId, string SellerName);

    public class Handler : IRequestHandler<Query, List<PendingMotorcycleDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<PendingMotorcycleDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT m.Id, m.Name, m.Brand, m.Price, m.SellerId, u.FullName AS SellerName
                FROM Motorcycles m
                INNER JOIN Users u ON u.Id = m.SellerId
                WHERE m.Status = 'Pending'
                ORDER BY m.Id
                """;

            var result = await connection.QueryAsync<PendingMotorcycleDto>(sql);
            return result.ToList();
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/motorcycles/pending", async (ISender sender) =>
            Results.Ok(await sender.Send(new Query())))
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("GetPendingMotorcycles")
            .WithTags("Admin");

    }
}