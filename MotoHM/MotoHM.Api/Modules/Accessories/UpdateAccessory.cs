using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Accessories;

public static class UpdateAccessory
{
    public record UpdateAccessoryCommand(int Id, string Name, string Brand, decimal Price,
        int StockQty, string? ImageUrl, IReadOnlyList<string>? ImageUrls, int UserId, bool IsAdmin) : IRequest<bool>;

    public class Handler : IRequestHandler<UpdateAccessoryCommand, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(UpdateAccessoryCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.Accessories.FirstOrDefaultAsync(a => a.Id == request.Id && (request.IsAdmin || a.SellerId == request.UserId), cancellationToken);

            if (entity is null)
                return false;

            entity.Name = request.Name;
            entity.Brand = request.Brand;
            entity.Price = request.Price;
            entity.StockQty = request.StockQty;
            var images = ImageList.Normalize(request.ImageUrls, request.ImageUrl);
            entity.ImageUrl = images.FirstOrDefault();
            entity.ImageUrlsJson = ImageList.ToJson(images);
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

    public record UpdateAccessoryBody(string Name, string Brand, decimal Price, int StockQty, string? ImageUrl,
        string[]? ImageUrls = null);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/accessories/{id:int}", async (int id, UpdateAccessoryBody body, ClaimsPrincipal user, ISender sender) =>
        {
            var command = new UpdateAccessoryCommand(id, body.Name, body.Brand, body.Price, body.StockQty, body.ImageUrl, body.ImageUrls,
                user.GetUserId(), user.IsInRole("Admin"));
            var success = await sender.Send(command);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("UpdateAccessory")
        .WithTags("Accessories");
    }
    public class Validator : AbstractValidator<UpdateAccessoryCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Brand).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.StockQty).GreaterThanOrEqualTo(0);
        }
    }
}