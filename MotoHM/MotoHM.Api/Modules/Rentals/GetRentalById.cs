using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Rentals;

public static class GetRentalById
{
    public record Query(int Id) : IRequest<RentalDetailDto?>;

    public record RentalDetailDto(int Id, int MotorcycleId, string MotorcycleName, DateTime StartDate,
        DateTime EndDate, string Period, decimal TotalPrice, string CustomerName, string CustomerPhone);

    public class Handler : IRequestHandler<Query, RentalDetailDto?>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<RentalDetailDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT r.Id, r.MotorcycleId, m.Name AS MotorcycleName, r.StartDate, r.EndDate,
                       r.Period, r.TotalPrice, r.CustomerName, r.CustomerPhone
                FROM Rentals r
                INNER JOIN Motorcycles m ON m.Id = r.MotorcycleId
                WHERE r.Id = @Id
                """;

            return await connection.QueryFirstOrDefaultAsync<RentalDetailDto>(sql, new { request.Id });
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/rentals/{id:int}", async (int id, ISender sender) =>
        {
            var result = await sender.Send(new Query(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetRentalById")
        .WithTags("Rentals");
    }
}