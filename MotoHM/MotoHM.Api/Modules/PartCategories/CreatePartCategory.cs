using MediatR;
using MotoHM.Api.Data;
using MotoHM.Api.Entites;

namespace MotoHM.Api.Modules.PartCategories;

public static class CreatePartCategory
{
    public record CreatePartCategoryCommand(string Icon, string Name) : IRequest<int>;

    public class Handler : IRequestHandler<CreatePartCategoryCommand, int>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<int> Handle(CreatePartCategoryCommand request, CancellationToken cancellationToken)
        {
            var entity = new PartCategoryEntity { Icon = request.Icon, Name = request.Name };
            _db.PartCategories.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/part-categories", async (CreatePartCategoryCommand command, ISender sender) =>
        {
            var id = await sender.Send(command);
            return Results.Created($"/api/part-categories/{id}", new { id });
        })
        .WithName("CreatePartCategory")
        .WithTags("PartCategories");
    }
}