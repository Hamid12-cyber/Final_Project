using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Entites;
using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Exceptions.Common;
using MotoHM.Api.Shared.Extensions;
using System.Security.Claims;

namespace MotoHM.Api.Modules.Reviews;

public static class CreateOrUpdateReview
{
    public record Command(int UserId, int? MotorcycleId, int? PartId, int? AccessoryId, int Rating, string? Comment)
        : IRequest<int>;

    public class Handler : IRequestHandler<Command, int>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<int> Handle(Command request, CancellationToken cancellationToken)
        {
            var providedCount = new[] { request.MotorcycleId, request.PartId, request.AccessoryId }
                .Count(x => x.HasValue);

            if (providedCount != 1)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Ya MotorcycleId, ya PartId, ya da AccessoryId göndərilməlidir (yalnız biri).");

            if (request.Rating < 1 || request.Rating > 5)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Reytinq 1 ilə 5 arasında olmalıdır.");

            var comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim();
            if (comment is { Length: > 1000 })
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Şərh 1000 simvoldan uzun ola bilməz.");

            // Rəy yalnız mövcud və təsdiqlənmiş məhsula yazıla bilər
            bool exists;
            if (request.MotorcycleId is not null)
                exists = await _db.Motorcycles.AnyAsync(
                    m => m.Id == request.MotorcycleId && m.Status == ApprovalStatus.Approved, cancellationToken);
            else if (request.PartId is not null)
                exists = await _db.Parts.AnyAsync(
                    p => p.Id == request.PartId && p.Status == ApprovalStatus.Approved, cancellationToken);
            else
                exists = await _db.Accessories.AnyAsync(
                    a => a.Id == request.AccessoryId && a.Status == ApprovalStatus.Approved, cancellationToken);

            if (!exists)
                throw new AppException("NOT_FOUND", StatusCodes.Status404NotFound, "Məhsul tapılmadı.");

            // Hər istifadəçi hər məhsula bir rəy yaza bilər, təkrar göndərəndə rəy yenilənir
            var review = await _db.Reviews.FirstOrDefaultAsync(r =>
                r.UserId == request.UserId &&
                r.MotorcycleId == request.MotorcycleId &&
                r.PartId == request.PartId &&
                r.AccessoryId == request.AccessoryId, cancellationToken);

            if (review is null)
            {
                review = new ReviewEntity
                {
                    UserId = request.UserId,
                    MotorcycleId = request.MotorcycleId,
                    PartId = request.PartId,
                    AccessoryId = request.AccessoryId,
                    Rating = request.Rating,
                    Comment = comment
                };
                _db.Reviews.Add(review);
            }
            else
            {
                review.Rating = request.Rating;
                review.Comment = comment;
                review.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return review.Id;
        }
    }

    public record ReviewBody(int? MotorcycleId, int? PartId, int? AccessoryId, int Rating, string? Comment);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/reviews", async (ReviewBody body, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.GetUserId();
            var id = await sender.Send(new Command(userId, body.MotorcycleId, body.PartId, body.AccessoryId,
                body.Rating, body.Comment));
            return Results.Ok(new { id });
        })
        .RequireAuthorization()
        .WithName("CreateOrUpdateReview")
        .WithTags("Reviews");
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Rating).GreaterThan(0);
        }
    }
}