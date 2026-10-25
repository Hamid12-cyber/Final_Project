using System.Security.Claims;
using Dapper;
using MediatR;
using MotoHM.Api.Data;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Favorites;

public static class GetMyFavorites
{
    public record Query(int UserId) : IRequest<List<FavoriteDto>>;

    public record FavoriteDto(int Id, string Name, string Brand, int Year, decimal Price, string? ImageUrl);

    public class Handler : IRequestHandler<Query, List<FavoriteDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<List<FavoriteDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT m.Id, m.Name, m.Brand, m.Year, m.Price, m.ImageUrl
                FROM Favorites f
                INNER JOIN Motorcycles m ON m.Id = f.MotorcycleId
                WHERE f.UserId = @UserId AND f.IsDeleted = 0
                  AND m.IsDeleted = 0 AND m.Status = 'Approved'
                ORDER BY f.CreatedAt DESC
                """;

            var result = await connection.QueryAsync<FavoriteDto>(sql, new { request.UserId });
            return result.ToList();
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/favorites", async (ClaimsPrincipal user, ISender sender) =>
            Results.Ok(await sender.Send(new Query(user.GetUserId()))))
            .RequireAuthorization()
            .WithName("GetMyFavorites")
            .WithTags("Favorites");
    }
}