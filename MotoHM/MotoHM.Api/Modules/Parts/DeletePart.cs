using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Parts;

public static class DeletePart
{
    public record DeletePartCommand(int Id) : IRequest<bool>;

    public class Handler : IRequestHandler<DeletePartCommand, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;
        public async Task<bool> Handle(DeletePartCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.Parts.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

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
        app.MapDelete("/api/parts/{id:int}", async (int id, ISender sender) =>
        {
            var success = await sender.Send(new DeletePartCommand(id));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeletePart")
        .WithTags("Parts");
    }
}