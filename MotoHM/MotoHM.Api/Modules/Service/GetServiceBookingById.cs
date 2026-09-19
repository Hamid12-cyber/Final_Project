using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Service;

public static class GetServiceBookingById
{
    public record Query(int Id) : IRequest<ServiceBookingDetailDto?>;

    public record ServiceBookingDetailDto(int Id, int MotorcycleId, string MotorcycleName, string Type,
        DateTime ScheduledDate, string CustomerName, string CustomerPhone, string? Notes);

    public class Handler : IRequestHandler<Query, ServiceBookingDetailDto?>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<ServiceBookingDetailDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT s.Id, s.MotorcycleId, m.Name AS MotorcycleName, s.Type,
                       s.ScheduledDate, s.CustomerName, s.CustomerPhone, s.Notes
                FROM ServiceBookings s
                INNER JOIN Motorcycles m ON m.Id = s.MotorcycleId
                WHERE s.Id = @Id
                """;

            return await connection.QueryFirstOrDefaultAsync<ServiceBookingDetailDto>(sql, new { request.Id });
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/service-bookings/{id:int}", async (int id, ISender sender) =>
        {
            var result = await sender.Send(new Query(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetServiceBookingById")
        .WithTags("Service");
    }
}