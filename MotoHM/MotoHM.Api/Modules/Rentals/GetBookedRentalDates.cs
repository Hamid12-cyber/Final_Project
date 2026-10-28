
namespace MotoHM.Api.Modules.Rentals;

public static class GetBookedRentalDates
{
    public record Query(int MotorcycleId) : IRequest<List<BookedRangeDto>>;

    // Yalnız tarixlər qaytarılır, müştəri adı/telefonu açıqlanmır.
    public record BookedRangeDto(DateTime StartDate, DateTime EndDate);

    public class Handler : IRequestHandler<Query, List<BookedRangeDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<BookedRangeDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT StartDate, EndDate
                FROM Rentals
                WHERE MotorcycleId = @MotorcycleId
                  AND EndDate > @Today
                ORDER BY StartDate
                """;

            var result = await connection.QueryAsync<BookedRangeDto>(
                sql, new { request.MotorcycleId, Today = DateTime.Today });
            return result.ToList();
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/rentals/booked/{motorcycleId:int}", async (int motorcycleId, ISender sender) =>
            Results.Ok(await sender.Send(new Query(motorcycleId))))
            .WithName("GetBookedRentalDates")
            .WithTags("Rentals");
    }
}