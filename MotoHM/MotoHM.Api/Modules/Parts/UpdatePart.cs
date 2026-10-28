using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Parts;

public static class UpdatePart
{
    public record UpdatePartCommand(int Id, string Name, string Brand, decimal Price, int StockQty,
        string? ImageUrl, IReadOnlyList<string>? ImageUrls, int PartCategoryId, int UserId, bool IsAdmin) : IRequest<bool>;

    public class Handler : IRequestHandler<UpdatePartCommand, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(UpdatePartCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.Parts.FirstOrDefaultAsync(p => p.Id == request.Id && (request.IsAdmin || p.SellerId == request.UserId), cancellationToken);

            if (entity is null)
                return false;

            entity.Name = request.Name;
            entity.Brand = request.Brand;
            entity.Price = request.Price;
            entity.StockQty = request.StockQty;
            var images = ImageList.Normalize(request.ImageUrls, request.ImageUrl);
            entity.ImageUrl = images.FirstOrDefault();
            entity.ImageUrlsJson = ImageList.ToJson(images);
            entity.PartCategoryId = request.PartCategoryId;
            // Satıcı elanı dəyişəndə (rədd edilmiş də olsa, təsdiqlənmiş də) yenidən admin təsdiqinə göndərilir
            if (!request.IsAdmin)
            {
                entity.Status = MotoHM.Api.Entites.Enums.ApprovalStatus.Pending;
                entity.RejectionReason = null;
            }

            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/parts/{id:int}", async (int id, UpdatePartBody body, ClaimsPrincipal user, ISender sender) =>
        {
            var command = new UpdatePartCommand(id, body.Name, body.Brand, body.Price, body.StockQty,
                body.ImageUrl, body.ImageUrls, body.PartCategoryId, user.GetUserId(), user.IsInRole("Admin"));

            var success = await sender.Send(command);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("UpdatePart")
        .WithTags("Parts");
    }

    public record UpdatePartBody(string Name, string Brand, decimal Price, int StockQty,
        string? ImageUrl, int PartCategoryId, string[]? ImageUrls = null);

    public class Validator : AbstractValidator<UpdatePartCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Brand).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.StockQty).GreaterThanOrEqualTo(0);
            RuleFor(x => x.PartCategoryId).GreaterThan(0);
        }
    }
}