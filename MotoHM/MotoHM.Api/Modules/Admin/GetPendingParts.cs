using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Admin;

public static class GetPendingParts
{
    public record Query : IRequest<List<PendingPartDto>>;

    public record PendingPartDto(int Id, string Name, string Brand, decimal Price, int SellerId, string SellerName);

    public class Handler : IRequestHandler<Query, List<PendingPartDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<PendingPartDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT p.Id, p.Name, p.Brand, p.Price, p.SellerId, u.FullName AS SellerName
                FROM Parts p
                INNER JOIN Users u ON u.Id = p.SellerId
                WHERE p.Status = 'Pending'
                ORDER BY p.Id
                """;

            var result = await connection.QueryAsync<PendingPartDto>(sql);
            return result.ToList();
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/parts/pending", async (ISender sender) =>
            Results.Ok(await sender.Send(new Query())))
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("GetPendingParts")
            .WithTags("Admin");
    }
}