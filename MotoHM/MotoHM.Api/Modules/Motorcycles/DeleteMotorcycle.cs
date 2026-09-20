using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Motorcycles;

public static class DeleteMotorcycle
{
    public record DeleteMotorcycleCommand(int Id) : IRequest<bool>;

    public class Handler : IRequestHandler<DeleteMotorcycleCommand, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;
        public async Task<bool> Handle(DeleteMotorcycleCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.Motorcycles.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (entity is null)
                return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/motorcycles/{id:int}", async (int id, ISender sender) =>
        {
            var success = await sender.Send(new DeleteMotorcycleCommand(id));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteMotorcycle")
        .WithTags("Motorcycles");
    }
}