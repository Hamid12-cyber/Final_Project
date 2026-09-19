using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Service;

public static class GetAvailableSlots
{
    // İş saatları: 09:00 - 19:00, hər saat bir slot
    private static readonly TimeSpan WorkStart = TimeSpan.FromHours(9);
    private static readonly TimeSpan WorkEnd = TimeSpan.FromHours(19);

    public record Query(DateOnly Date) : IRequest<List<SlotDto>>;

    public record SlotDto(DateTime Time, bool IsAvailable);

    public class Handler : IRequestHandler<Query, List<SlotDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<SlotDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT ScheduledDate
                FROM ServiceBookings
                WHERE CAST(ScheduledDate AS DATE) = @Date
                """;

            var bookedTimes = (await connection.QueryAsync<DateTime>(sql, new { Date = request.Date.ToDateTime(TimeOnly.MinValue) }))
                .ToHashSet();

            var slots = new List<SlotDto>();
            var current = request.Date.ToDateTime(TimeOnly.FromTimeSpan(WorkStart));
            var end = request.Date.ToDateTime(TimeOnly.FromTimeSpan(WorkEnd));

            while (current < end)
            {
                slots.Add(new SlotDto(current, !bookedTimes.Contains(current)));
                current = current.AddHours(1);
            }

            return slots;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/service-bookings/available-slots", async (DateOnly date, ISender sender) =>
            Results.Ok(await sender.Send(new Query(date))))
            .WithName("GetAvailableSlots")
            .WithTags("Service");
    }
}