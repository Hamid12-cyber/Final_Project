using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.PartCategories;

public static class DeletePartCategory
{
    public record DeletePartCategoryCommand(int Id) : IRequest<bool>;

    public class Handler : IRequestHandler<DeletePartCategoryCommand, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(DeletePartCategoryCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.PartCategories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (entity is null)
                return false;

            _db.PartCategories.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/part-categories/{id:int}", async (int id, ISender sender) =>
        {
            var success = await sender.Send(new DeletePartCategoryCommand(id));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeletePartCategory")
        .WithTags("PartCategories");
    }
}