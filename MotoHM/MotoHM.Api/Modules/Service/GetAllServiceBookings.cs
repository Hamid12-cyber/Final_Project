using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Service;

public static class GetAllServiceBookings
{
    public record Query : IRequest<List<ServiceBookingDto>>;

    public record ServiceBookingDto(int Id, int MotorcycleId, string MotorcycleName, string Type,
        DateTime ScheduledDate, string CustomerName, string CustomerPhone, string? Notes);

    public class Handler : IRequestHandler<Query, List<ServiceBookingDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<ServiceBookingDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT s.Id, s.MotorcycleId, m.Name AS MotorcycleName, s.Type,
                       s.ScheduledDate, s.CustomerName, s.CustomerPhone, s.Notes
                FROM ServiceBookings s
                INNER JOIN Motorcycles m ON m.Id = s.MotorcycleId
                ORDER BY s.ScheduledDate
                """;

            var result = await connection.QueryAsync<ServiceBookingDto>(sql);
            return result.ToList();
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/service-bookings", async (ISender sender) =>
            Results.Ok(await sender.Send(new Query())))
            .WithName("GetAllServiceBookings")
            .WithTags("Service");
    }
}