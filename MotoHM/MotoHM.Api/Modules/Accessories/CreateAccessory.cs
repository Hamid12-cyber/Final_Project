using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Entites;
using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Exceptions.Common;
using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Accessories;

public static class CreateAccessory
{
    public record CreateAccessoryCommand(string Name, string Brand, decimal Price, int StockQty,
        string? ImageUrl, int SellerId) : IRequest<int>;

    public class Handler : IRequestHandler<CreateAccessoryCommand, int>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<int> Handle(CreateAccessoryCommand request, CancellationToken cancellationToken)
        {
            var sellerExists = await _db.Users.AnyAsync(u => u.Id == request.SellerId, cancellationToken);
            if (!sellerExists)
                throw new AppException("NOT_FOUND", StatusCodes.Status404NotFound,
                    $"Seller (Id={request.SellerId}) tapılmadı.");

            var entity = new AccessoryEntity
            {
                SellerId = request.SellerId,
                Name = request.Name,
                Brand = request.Brand,
                Price = request.Price,
                StockQty = request.StockQty,
                ImageUrl = request.ImageUrl,
                Status = ApprovalStatus.Pending
            };

            _db.Accessories.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }

    public record CreateAccessoryBody(string Name, string Brand, decimal Price, int StockQty, string? ImageUrl);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/accessories", async (CreateAccessoryBody body, ClaimsPrincipal user, ISender sender) =>
        {
            var sellerId = user.GetUserId();
            var command = new CreateAccessoryCommand(body.Name, body.Brand, body.Price, body.StockQty,
                body.ImageUrl, sellerId);

            var id = await sender.Send(command);
            return Results.Created($"/api/accessories/{id}", new { id });
        })
        .RequireAuthorization()
        .WithName("CreateAccessory")
        .WithTags("Accessories");
    }
}