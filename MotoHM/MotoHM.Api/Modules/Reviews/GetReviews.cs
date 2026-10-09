using System.Security.Claims;
using Dapper;
using MediatR;
using MotoHM.Api.Data;
using MotoHM.Api.Shared.Exceptions.Common;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Reviews;

public static class GetReviews
{
    public record Query(int? MotorcycleId, int? PartId, int? AccessoryId, int? CurrentUserId) : IRequest<Response>;

    public record ReviewDto(int Id, string UserName, int Rating, string? Comment, DateTime CreatedAt, bool IsMine);

    public record Response(double AverageRating, int Count, List<ReviewDto> Items);

    public record ReviewRow(int Id, int UserId, string UserName, int Rating, string? Comment, DateTime CreatedAt);

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public Handler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var providedCount = new[] { request.MotorcycleId, request.PartId, request.AccessoryId }
                .Count(x => x.HasValue);

            if (providedCount != 1)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Ya motorcycleId, ya partId, ya da accessoryId göndərilməlidir (yalnız biri).");

            // Sütun adı yalnız bu üç sabit dəyərdən biridir (istifadəçi girişi SQL-ə qarışmır)
            var (column, productId) =
                request.MotorcycleId is not null ? ("MotorcycleId", request.MotorcycleId.Value) :
                request.PartId is not null ? ("PartId", request.PartId.Value) :
                ("AccessoryId", request.AccessoryId!.Value);

            using var connection = _connectionFactory.CreateConnection();

            var sql = $"""
                SELECT r.Id, r.UserId, u.FullName AS UserName, r.Rating, r.Comment, r.CreatedAt
                FROM Reviews r
                INNER JOIN Users u ON u.Id = r.UserId
                WHERE r.IsDeleted = 0 AND r.{column} = @ProductId
                ORDER BY r.CreatedAt DESC
                """;

            var rows = (await connection.QueryAsync<ReviewRow>(sql, new { ProductId = productId })).ToList();

            var items = rows
                .Select(r => new ReviewDto(r.Id, ShortName(r.UserName), r.Rating, r.Comment, r.CreatedAt,
                    request.CurrentUserId is not null && request.CurrentUserId == r.UserId))
                .ToList();

            var average = items.Count == 0 ? 0 : Math.Round(items.Average(i => i.Rating), 1);

            return new Response(average, items.Count, items);
        }

        // Məxfilik üçün soyadın yalnız ilk hərfi göstərilir: "Əli Məmmədov" -> "Əli M."
        private static string ShortName(string fullName)
        {
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2 ? $"{parts[0]} {parts[^1][0]}." : fullName;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Hamı oxuya bilər, amma token varsa istifadəçinin öz rəyi "IsMine" ilə işarələnir
        app.MapGet("/api/reviews", async (int? motorcycleId, int? partId, int? accessoryId,
            ClaimsPrincipal user, ISender sender) =>
        {
            int? currentUserId = user.Identity?.IsAuthenticated == true ? user.GetUserId() : null;
            return Results.Ok(await sender.Send(new Query(motorcycleId, partId, accessoryId, currentUserId)));
        })
        .WithName("GetReviews")
        .WithTags("Reviews");
    }
}