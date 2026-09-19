using System.Security.Claims;
using Dapper;
using MediatR;
using MotoHM.Api.Data;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Cart;

public static class GetMyCart
{
    public record Query(int UserId) : IRequest<List<CartItemDto>>;

    public record CartItemDto(int Id, int? MotorcycleId, string? MotorcycleName, int? PartId,
        string? PartName, decimal UnitPrice, int Quantity);

    public class Handler : IRequestHandler<Query, List<CartItemDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<CartItemDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT ci.Id, ci.MotorcycleId, m.Name AS MotorcycleName, ci.PartId, p.Name AS PartName,
                       COALESCE(m.Price, p.Price) AS UnitPrice, ci.Quantity
                FROM CartItems ci
                INNER JOIN Carts c ON c.Id = ci.CartId
                LEFT JOIN Motorcycles m ON m.Id = ci.MotorcycleId
                LEFT JOIN Parts p ON p.Id = ci.PartId
                WHERE c.UserId = @UserId
                """;

            var result = await connection.QueryAsync<CartItemDto>(sql, new { request.UserId });
            return result.ToList();
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/cart", async (ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.GetUserId();
            var result = await sender.Send(new Query(userId));
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetMyCart")
        .WithTags("Cart");
    }
}