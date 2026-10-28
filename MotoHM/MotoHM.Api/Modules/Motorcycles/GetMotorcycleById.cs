using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Motorcycles;

public static class GetMotorcycleById
{
    public record Query(int Id) : IRequest<MotorcycleDetailDto?>;

    public record MotorcycleDetailDto(int Id, string Name, string Brand, string Model, int Cc, int Year,
        decimal Price, string? ImageUrl, IReadOnlyList<string> ImageUrls, bool IsForRent, bool IsForSale);

    public record MotorcycleRow(int Id, string Name, string Brand, string Model, int Cc, int Year,
        decimal Price, string? ImageUrl, string? ImageUrlsJson, bool IsForRent, bool IsForSale);

    public class Handler : IRequestHandler<Query, MotorcycleDetailDto?>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<MotorcycleDetailDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT Id, Name, Brand, Model, Cc, Year, Price, ImageUrl, ImageUrlsJson, IsForRent, IsForSale
                FROM Motorcycles
                WHERE Id = @Id AND IsDeleted = 0
                """;

            var row = await connection.QueryFirstOrDefaultAsync<MotorcycleRow>(sql, new { request.Id });
            if (row is null)
                return null;

            return new MotorcycleDetailDto(row.Id, row.Name, row.Brand, row.Model, row.Cc, row.Year,
                row.Price, row.ImageUrl, ImageList.FromJson(row.ImageUrlsJson, row.ImageUrl),
                row.IsForRent, row.IsForSale);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/motorcycles/{id:int}", async (int id, ISender sender) =>
        {
            var result = await sender.Send(new Query(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetMotorcycleById")
        .WithTags("Motorcycles");
    }
}