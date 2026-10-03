using Dapper;
using MediatR;
using MotoHM.Api.Data;
using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Exceptions.Common;

namespace MotoHM.Api.Modules.Admin;

public static class GetAllOrders
{
    public record Query(string? Status, int Page, int PageSize) : IRequest<Response>;

    public record AdminOrderDto(int Id, string Status, decimal TotalAmount, DateTime CreatedAt,
        int UserId, string CustomerName, string CustomerEmail,
        string ShippingAddress, string ContactPhone, int ItemCount);

    public record Response(List<AdminOrderDto> Items, int TotalCount, int Page, int PageSize);

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            string? status = null;
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var parsed) ||
                    !Enum.IsDefined(parsed))
                    throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                        $"Yanlış status: '{request.Status}'. Mümkün dəyərlər: Pending, Confirmed, Shipped, Delivered, Cancelled.");

                status = parsed.ToString();
            }

            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT o.Id, o.Status, o.TotalAmount, o.CreatedAt, o.UserId,
                       u.FullName AS CustomerName, u.Email AS CustomerEmail,
                       o.ShippingAddress, o.ContactPhone,
                       (SELECT COALESCE(SUM(oi.Quantity), 0) FROM OrderItems oi WHERE oi.OrderId = o.Id) AS ItemCount
                FROM Orders o
                INNER JOIN Users u ON u.Id = o.UserId
                WHERE o.IsDeleted = 0 AND (@Status IS NULL OR o.Status = @Status)
                ORDER BY o.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM Orders o
                WHERE o.IsDeleted = 0 AND (@Status IS NULL OR o.Status = @Status);
                """;

            using var multi = await connection.QueryMultipleAsync(sql,
                new { Status = status, Offset = (page - 1) * pageSize, PageSize = pageSize });

            var items = (await multi.ReadAsync<AdminOrderDto>()).ToList();
            var totalCount = await multi.ReadSingleAsync<int>();

            return new Response(items, totalCount, page, pageSize);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/orders", async (string? status, int? page, int? pageSize, ISender sender) =>
            Results.Ok(await sender.Send(new Query(status, page ?? 1, pageSize ?? 50))))
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("GetAllOrdersAdmin")
            .WithTags("Admin");
    }
}