using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Parts;

public static class UpdatePart
{
    public record UpdatePartCommand(int Id, string Name, string Brand, decimal Price, int StockQty,
        string? ImageUrl, int PartCategoryId) : IRequest<bool>;

    public class Handler : IRequestHandler<UpdatePartCommand, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(UpdatePartCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.Parts.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (entity is null)
                return false;

            entity.Name = request.Name;
            entity.Brand = request.Brand;
            entity.Price = request.Price;
            entity.StockQty = request.StockQty;
            entity.ImageUrl = request.ImageUrl;
            entity.PartCategoryId = request.PartCategoryId;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/parts/{id:int}", async (int id, UpdatePartBody body, ISender sender) =>
        {
            var command = new UpdatePartCommand(id, body.Name, body.Brand, body.Price, body.StockQty,
                body.ImageUrl, body.PartCategoryId);

            var success = await sender.Send(command);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdatePart")
        .WithTags("Parts");
    }

    public record UpdatePartBody(string Name, string Brand, decimal Price, int StockQty,
        string? ImageUrl, int PartCategoryId);
}