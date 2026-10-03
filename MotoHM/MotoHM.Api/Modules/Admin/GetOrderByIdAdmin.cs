using Dapper;
using MediatR;
using MotoHM.Api.Data;
using MotoHM.Api.Modules.Orders;
using MotoHM.Api.Shared.Exceptions.Common;

namespace MotoHM.Api.Modules.Admin;

public static class GetOrderByIdAdmin
{
    public record Query(int Id) : IRequest<AdminOrderDetailDto>;

    public record AdminOrderDetailDto(int Id, string Status, decimal TotalAmount, string ShippingAddress,
        string ContactPhone, DateTime CreatedAt, int UserId, string CustomerName, string CustomerEmail,
        List<GetOrderById.OrderItemDto> Items);

    public class Handler : IRequestHandler<Query, AdminOrderDetailDto>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<AdminOrderDetailDto> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string orderSql = """
                SELECT o.Id, o.Status, o.TotalAmount, o.ShippingAddress, o.ContactPhone, o.CreatedAt,
                       o.UserId, u.FullName AS CustomerName, u.Email AS CustomerEmail
                FROM Orders o
                INNER JOIN Users u ON u.Id = o.UserId
                WHERE o.Id = @Id AND o.IsDeleted = 0
                """;

            var order = await connection.QueryFirstOrDefaultAsync(orderSql, new { request.Id });

            if (order is null)
                throw new AppException("NOT_FOUND", StatusCodes.Status404NotFound,
                    $"Order (Id={request.Id}) tapılmadı.");

            const string itemsSql = """
                SELECT oi.MotorcycleId, m.Name AS MotorcycleName, oi.PartId, p.Name AS PartName,
                       oi.AccessoryId, a.Name AS AccessoryName,
                       oi.Quantity, oi.UnitPriceAtOrderTime
                FROM OrderItems oi
                LEFT JOIN Motorcycles m ON m.Id = oi.MotorcycleId
                LEFT JOIN Parts p ON p.Id = oi.PartId
                LEFT JOIN Accessories a ON a.Id = oi.AccessoryId
                WHERE oi.OrderId = @Id
                """;

            var items = (await connection.QueryAsync<GetOrderById.OrderItemDto>(itemsSql, new { request.Id })).ToList();

            return new AdminOrderDetailDto((int)order.Id, (string)order.Status, (decimal)order.TotalAmount,
                (string)order.ShippingAddress, (string)order.ContactPhone, (DateTime)order.CreatedAt,
                (int)order.UserId, (string)order.CustomerName, (string)order.CustomerEmail, items);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/orders/{id:int}", async (int id, ISender sender) =>
            Results.Ok(await sender.Send(new Query(id))))
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("GetOrderByIdAdmin")
            .WithTags("Admin");
    }
}