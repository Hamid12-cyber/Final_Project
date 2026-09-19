using System.Security.Claims;
using Dapper;
using MediatR;
using MotoHM.Api.Data;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Orders;

public static class GetMyOrders
{
    public record Query(int UserId) : IRequest<List<OrderSummaryDto>>;

    public record OrderSummaryDto(int Id, string Status, decimal TotalAmount, DateTime CreatedAt);

    public class Handler : IRequestHandler<Query, List<OrderSummaryDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<OrderSummaryDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT Id, Status, TotalAmount, CreatedAt
                FROM Orders
                WHERE UserId = @UserId
                ORDER BY CreatedAt DESC
                """;

            var result = await connection.QueryAsync<OrderSummaryDto>(sql, new { request.UserId });
            return result.ToList();
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders", async (ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.GetUserId();
            var result = await sender.Send(new Query(userId));
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetMyOrders")
        .WithTags("Orders");
    }
}