using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Parts;

public static class CreatePart
{
    public record CreatePartCommand(string Name, string Brand, decimal Price, int StockQty,
        string? ImageUrl, IReadOnlyList<string>? ImageUrls, int PartCategoryId, int SellerId) : IRequest<int>;

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

            var categoryExists = await _db.PartCategories.AnyAsync(c => c.Id == request.PartCategoryId, cancellationToken);
            if (!categoryExists)
                throw new AppException("NOT_FOUND", StatusCodes.Status404NotFound,
                    $"PartCategory (Id={request.PartCategoryId}) tapılmadı.");

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
                PartCategoryId = request.PartCategoryId,
                Status = ApprovalStatus.Pending
            };

            _db.Parts.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }

    public record CreatePartBody(string Name, string Brand, decimal Price, int StockQty,
        string? ImageUrl, int PartCategoryId, string[]? ImageUrls = null);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/parts", async (CreatePartBody body, ClaimsPrincipal user, ISender sender) =>
        {
            var sellerId = user.GetUserId();

            var command = new CreatePartCommand(body.Name, body.Brand, body.Price, body.StockQty,
                body.ImageUrl, body.ImageUrls, body.PartCategoryId, sellerId);

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
            RuleFor(x => x.PartCategoryId).GreaterThan(0);
        }
    }
}