using System.Security.Claims;
using Dapper;
using MediatR;
using MotoHM.Api.Data;
using MotoHM.Api.Shared.Exceptions.Common;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Orders;

public static class GetOrderById
{
    public record Query(int Id, int UserId) : IRequest<OrderDetailDto>;

    public record OrderDetailDto(int Id, string Status, decimal TotalAmount, string ShippingAddress,
        string ContactPhone, DateTime CreatedAt, List<OrderItemDto> Items);

    public record OrderItemDto(int? MotorcycleId, string? MotorcycleName, int? PartId, string? PartName,
        int Quantity, decimal UnitPriceAtOrderTime);

    public class Handler : IRequestHandler<Query, OrderDetailDto>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<OrderDetailDto> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string orderSql = """
                SELECT Id, Status, TotalAmount, ShippingAddress, ContactPhone, CreatedAt
                FROM Orders
                WHERE Id = @Id AND UserId = @UserId
                """;

            var order = await connection.QueryFirstOrDefaultAsync(orderSql, new { request.Id, request.UserId });

            if (order is null)
                throw new AppException("NOT_FOUND", StatusCodes.Status404NotFound,
                    $"Order (Id={request.Id}) tapılmadı.");

            const string itemsSql = """
                SELECT oi.MotorcycleId, m.Name AS MotorcycleName, oi.PartId, p.Name AS PartName,
                       oi.Quantity, oi.UnitPriceAtOrderTime
                FROM OrderItems oi
                LEFT JOIN Motorcycles m ON m.Id = oi.MotorcycleId
                LEFT JOIN Parts p ON p.Id = oi.PartId
                WHERE oi.OrderId = @Id
                """;

            var items = (await connection.QueryAsync<OrderItemDto>(itemsSql, new { request.Id })).ToList();

            return new OrderDetailDto(order.Id, order.Status, order.TotalAmount, order.ShippingAddress,
                order.ContactPhone, order.CreatedAt, items);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{id:int}", async (int id, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.GetUserId();
            var result = await sender.Send(new Query(id, userId));
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetOrderById")
        .WithTags("Orders");
    }
}