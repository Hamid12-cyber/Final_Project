using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Rentals;

public static class GetAllRentals
{
    public record Query : IRequest<List<RentalDto>>;

    public record RentalDto(int Id, int MotorcycleId, string MotorcycleName, DateTime StartDate,
        DateTime EndDate, string Period, decimal TotalPrice, string CustomerName, string CustomerPhone);

    public class Handler : IRequestHandler<Query, List<RentalDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<RentalDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT r.Id, r.MotorcycleId, m.Name AS MotorcycleName, r.StartDate, r.EndDate,
                       r.Period, r.TotalPrice, r.CustomerName, r.CustomerPhone
                FROM Rentals r
                INNER JOIN Motorcycles m ON m.Id = r.MotorcycleId
                ORDER BY r.Id DESC
                """;

            var result = await connection.QueryAsync<RentalDto>(sql);
            return result.ToList();
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/rentals", async (ISender sender) =>
            Results.Ok(await sender.Send(new Query())))
            .WithName("GetAllRentals")
            .WithTags("Rentals");
    }
}