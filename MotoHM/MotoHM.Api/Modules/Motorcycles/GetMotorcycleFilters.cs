using Dapper;
using MediatR;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Motorcycles;

public static class GetMotorcycleFilters
{
    public record Query(string? Brand, string? Model, int? Year) : IRequest<Response>;

    public record Option(string Value, string Label, int Count);

    public record Response(List<Option> Brands, List<Option> Models, List<Option> Years, List<Option> PriceRanges);

    public record Row(string Brand, string? Model, int Year, decimal Price);

    private record PriceRange(string Value, string Label, decimal Min, decimal Max);

    private static readonly PriceRange[] PriceRanges =
    {
        new("0-5000", "5 000 AZN-ə qədər", 0, 5000),
        new("5000-10000", "5 000 – 10 000 AZN", 5000, 10000),
        new("10000-20000", "10 000 – 20 000 AZN", 10000, 20000),
        new("20000-40000", "20 000 – 40 000 AZN", 20000, 40000),
        new("40000-", "40 000 AZN-dən yuxarı", 40000, decimal.MaxValue),
    };

    private static bool Eq(string? a, string? b) =>
        string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT Brand, Model, Year, Price
                FROM Motorcycles
                WHERE Status = 'Approved' AND IsDeleted = 0
                """;

            var bikes = (await connection.QueryAsync<Row>(sql)).ToList();

            bool hasBrand = !string.IsNullOrWhiteSpace(request.Brand);
            bool hasModel = !string.IsNullOrWhiteSpace(request.Model);

            // Markalar: katalog + bazada olan, say hamısı üzrə
            var brandNames = MotorcycleCatalog.Brands.Keys
                .Concat(bikes.Select(b => b.Brand.Trim()))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            var brands = brandNames
                .Select(n => new Option(n, n, bikes.Count(b => Eq(b.Brand, n))))
                .OrderByDescending(o => o.Count).ThenBy(o => o.Label)
                .ToList();

            // Modellər: yalnız marka seçiləndə, o markanın daxilində
            var models = new List<Option>();
            if (hasBrand)
            {
                var brandBikes = bikes.Where(b => Eq(b.Brand, request.Brand)).ToList();
                var catalogModels = MotorcycleCatalog.Brands.TryGetValue(request.Brand!.Trim(), out var m)
                    ? m : Array.Empty<string>();

                models = catalogModels
                    .Concat(brandBikes.Select(b => (b.Model ?? "").Trim()).Where(x => x.Length > 0))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(n => new Option(n, n, brandBikes.Count(b => Eq(b.Model, n))))
                    .OrderByDescending(o => o.Count).ThenBy(o => o.Label)
                    .ToList();
            }

            // İllər: seçilmiş marka/model daxilində
            var yearScope = bikes
                .Where(b => !hasBrand || Eq(b.Brand, request.Brand))
                .Where(b => !hasModel || Eq(b.Model, request.Model))
                .ToList();

            int maxYear = DateTime.Now.Year + 1;
            int minYear = Math.Min(1990, bikes.Count > 0 ? bikes.Min(b => b.Year) : 1990);
            var years = Enumerable.Range(minYear, maxYear - minYear + 1)
                .Reverse()
                .Select(y => new Option(y.ToString(), y.ToString(), yearScope.Count(b => b.Year == y)))
                .ToList();

            // Qiymət aralıqları: marka/model/il daxilində
            var priceScope = yearScope
                .Where(b => request.Year is null || b.Year == request.Year)
                .ToList();

            var prices = PriceRanges
                .Select(r => new Option(r.Value, r.Label, priceScope.Count(b => b.Price >= r.Min && b.Price < r.Max)))
                .ToList();

            return new Response(brands, models, years, prices);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/motorcycles/filters", async (string? brand, string? model, int? year, ISender sender) =>
            Results.Ok(await sender.Send(new Query(brand, model, year))))
            .WithName("GetMotorcycleFilters")
            .WithTags("Motorcycles");
    }
}