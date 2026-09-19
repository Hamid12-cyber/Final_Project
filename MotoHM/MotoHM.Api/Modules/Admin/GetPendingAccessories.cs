using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Admin;

public static class GetPendingAccessories
{
    public record Query : IRequest<List<PendingAccessoryDto>>;

    public record PendingAccessoryDto(int Id, string Name, string Brand, decimal Price, int SellerId, string SellerName);

    public class Handler : IRequestHandler<Query, List<PendingAccessoryDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<PendingAccessoryDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT a.Id, a.Name, a.Brand, a.Price, a.SellerId, u.FullName AS SellerName
                FROM Accessories a
                INNER JOIN Users u ON u.Id = a.SellerId
                WHERE a.Status = 'Pending'
                ORDER BY a.Id
                """;

            var result = await connection.QueryAsync<PendingAccessoryDto>(sql);
            return result.ToList();
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/accessories/pending", async (ISender sender) =>
            Results.Ok(await sender.Send(new Query())))
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("GetPendingAccessories")
            .WithTags("Admin");
    }
}