using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Motorcycles;

public static class CreateMotorcycle
{
    public record CreateMotorcycleCommand(string Name, string Brand, string Model, int Cc,
        int Year, decimal Price, string? ImageUrl, IReadOnlyList<string>? ImageUrls,
        bool IsForRent, bool IsForSale, int SellerId) : IRequest<int>;

    public class Handler : IRequestHandler<CreateMotorcycleCommand, int>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<int> Handle(CreateMotorcycleCommand request, CancellationToken cancellationToken)
        {
            var sellerExists = await _db.Users.AnyAsync(u => u.Id == request.SellerId, cancellationToken);
            if (!sellerExists)
                throw new AppException("NOT_FOUND", StatusCodes.Status404NotFound,
                    $"Seller (Id={request.SellerId}) tapılmadı.");

            var images = ImageList.Normalize(request.ImageUrls, request.ImageUrl);

            var entity = new MotorcycleEntity
            {
                SellerId = request.SellerId,
                Name = request.Name,
                Brand = request.Brand,
                Model = request.Model,
                Cc = request.Cc,
                Year = request.Year,
                Price = request.Price,
                ImageUrl = images.FirstOrDefault(),
                ImageUrlsJson = ImageList.ToJson(images),
                IsForRent = request.IsForRent,
                IsForSale = request.IsForSale,
                Status = ApprovalStatus.Pending
            };

            _db.Motorcycles.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }

    public record CreateMotorcycleBody(string Name, string Brand, string Model, int Cc,
        int Year, decimal Price, string? ImageUrl,
        bool IsForRent = false, bool IsForSale = true, string[]? ImageUrls = null);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/motorcycles", async (CreateMotorcycleBody body, ClaimsPrincipal user, ISender sender) =>
        {
            var sellerId = user.GetUserId();

            var command = new CreateMotorcycleCommand(body.Name, body.Brand, body.Model, body.Cc,
                body.Year, body.Price, body.ImageUrl, body.ImageUrls, body.IsForRent, body.IsForSale, sellerId);

            var id = await sender.Send(command);
            return Results.Created($"/api/motorcycles/{id}", new { id });
        })
        .RequireAuthorization()
        .WithName("CreateMotorcycle")
        .WithTags("Motorcycles");
    }
    public class Validator : AbstractValidator<CreateMotorcycleCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Brand).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Model).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Cc).GreaterThan(0);
            RuleFor(x => x.Year).InclusiveBetween(1980, DateTime.UtcNow.Year + 1);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x).Must(x => x.IsForRent || x.IsForSale)
                .WithMessage("Elan ən azı satış və ya kirayə üçün olmalıdır.");
        }
    }
}