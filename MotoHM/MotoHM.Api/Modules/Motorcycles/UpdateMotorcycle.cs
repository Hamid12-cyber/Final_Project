using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Motorcycles;

public static class UpdateMotorcycle
{
    public record UpdateMotorcycleCommand(int Id, string Name, string Brand, string Model, int Cc, int Year,
    decimal Price, string? ImageUrl, bool IsForRent, bool IsForSale) : IRequest<bool>;

    public class Handler : IRequestHandler<UpdateMotorcycleCommand, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(UpdateMotorcycleCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.Motorcycles.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (entity is null)
                return false;

            entity.Name = request.Name;
            entity.Brand = request.Brand;
            entity.Model = request.Model;
            entity.Cc = request.Cc;
            entity.Year = request.Year;
            entity.Price = request.Price;
            entity.ImageUrl = request.ImageUrl;
            entity.IsForRent = request.IsForRent;
            entity.IsForSale = request.IsForSale;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/motorcycles/{id:int}", async (int id, UpdateMotorcycleBody body, ISender sender) =>
        {
            var command = new UpdateMotorcycleCommand(id, body.Name, body.Brand, body.Model, body.Cc, body.Year,
                body.Price, body.ImageUrl, body.IsForRent, body.IsForSale);

            var success = await sender.Send(command);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateMotorcycle")
        .WithTags("Motorcycles");
    }

    public record UpdateMotorcycleBody(string Name, string Brand, string Model, int Cc, int Year,
        decimal Price, string? ImageUrl, bool IsForRent, bool IsForSale);
}