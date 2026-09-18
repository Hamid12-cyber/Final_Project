using MediatR;
using MotoHM.Api.Data;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Modules.Motorcycles;

public static class CreateMotorcycle
{
    public record CreateMotorcycleCommand(string Name, string Brand, string Model, int Cc, int Year, decimal Price, string? ImageUrl)
    : IRequest<int>;

    public class Handler : IRequestHandler<CreateMotorcycleCommand, int>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<int> Handle(CreateMotorcycleCommand request, CancellationToken cancellationToken)
        {
            var entity = new MotorcycleEntity
            {
                Name = request.Name,
                Brand = request.Brand,
                Model = request.Model,
                Cc = request.Cc,
                Year = request.Year,
                Price = request.Price,
                ImageUrl = request.ImageUrl
            };

            _db.Motorcycles.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/motorcycles", async (CreateMotorcycleCommand command, ISender sender) =>
        {
            var id = await sender.Send(command);
            return Results.Created($"/api/motorcycles/{id}", new { id });
        })
        .WithName("CreateMotorcycle")
        .WithTags("Motorcycles");
    }
}
