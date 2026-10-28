using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Parts;

public static class CreatePart
{
    public record CreatePartCommand(string Name, string Brand, decimal Price, int StockQty,
        string? ImageUrl, IReadOnlyList<string>? ImageUrls, int PartCategoryId, int SellerId,
        string? CustomCategoryName = null) : IRequest<int>;

    public class Handler : IRequestHandler<CreatePartCommand, int>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<int> Handle(CreatePartCommand request, CancellationToken cancellationToken)
        {
            var sellerExists = await _db.Users.AnyAsync(u => u.Id == request.SellerId, cancellationToken);
            if (!sellerExists)
                throw new AppException("NOT_FOUND", StatusCodes.Status404NotFound,
                    $"Seller (Id={request.SellerId}) tapılmadı.");

            int categoryId;

            if (request.PartCategoryId > 0)
            {
                var categoryExists = await _db.PartCategories.AnyAsync(c => c.Id == request.PartCategoryId, cancellationToken);
                if (!categoryExists)
                    throw new AppException("NOT_FOUND", StatusCodes.Status404NotFound,
                        $"PartCategory (Id={request.PartCategoryId}) tapılmadı.");

                categoryId = request.PartCategoryId;
            }
            else
            {
                // "Digər" seçilib: istifadəçinin yazdığı kateqoriyanı tap, yoxdursa yarat
                var customName = string.Join(' ',
                    (request.CustomCategoryName ?? string.Empty)
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries));

                var lowered = customName.ToLower();
                var existing = await _db.PartCategories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == lowered, cancellationToken);

                if (existing is null)
                {
                    existing = new PartCategoryEntity { Icon = "📦", Name = customName };
                    _db.PartCategories.Add(existing);
                    await _db.SaveChangesAsync(cancellationToken);
                }

                categoryId = existing.Id;
            }

            var images = ImageList.Normalize(request.ImageUrls, request.ImageUrl);

            var entity = new PartEntity
            {
                SellerId = request.SellerId,
                Name = request.Name,
                Brand = request.Brand,
                Price = request.Price,
                StockQty = request.StockQty,
                ImageUrl = images.FirstOrDefault(),
                ImageUrlsJson = ImageList.ToJson(images),
                PartCategoryId = categoryId,
                Status = ApprovalStatus.Pending
            };

            _db.Parts.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }

    public record CreatePartBody(string Name, string Brand, decimal Price, int StockQty,
        string? ImageUrl, int PartCategoryId, string[]? ImageUrls = null,
        string? CustomCategoryName = null);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/parts", async (CreatePartBody body, ClaimsPrincipal user, ISender sender) =>
        {
            var sellerId = user.GetUserId();

            var command = new CreatePartCommand(body.Name, body.Brand, body.Price, body.StockQty,
                body.ImageUrl, body.ImageUrls, body.PartCategoryId, sellerId, body.CustomCategoryName);

            var id = await sender.Send(command);
            return Results.Created($"/api/parts/{id}", new { id });
        })
        .RequireAuthorization()
        .WithName("CreatePart")
        .WithTags("Parts");
    }

    public class Validator : AbstractValidator<CreatePartCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Brand).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.StockQty).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CustomCategoryName).MaximumLength(100);
            RuleFor(x => x)
                .Must(x => x.PartCategoryId > 0 || !string.IsNullOrWhiteSpace(x.CustomCategoryName))
                .WithMessage("Kateqoriya seçin və ya yazın.");
        }
    }
}