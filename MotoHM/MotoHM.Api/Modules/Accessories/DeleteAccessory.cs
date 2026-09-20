using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Accessories;

public static class DeleteAccessory
{
    public record DeleteAccessoryCommand(int Id) : IRequest<bool>;

    public class Handler : IRequestHandler<DeleteAccessoryCommand, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(DeleteAccessoryCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.Accessories.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

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
        app.MapDelete("/api/accessories/{id:int}", async (int id, ISender sender) =>
        {
            var success = await sender.Send(new DeleteAccessoryCommand(id));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("DeleteAccessory")
        .WithTags("Accessories");
    }
}