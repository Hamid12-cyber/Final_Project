using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.PartCategories;

public static class UpdatePartCategory
{
    public record UpdatePartCategoryCommand(int Id, string Icon, string Name) : IRequest<bool>;

    public class Handler : IRequestHandler<UpdatePartCategoryCommand, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(UpdatePartCategoryCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.PartCategories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (entity is null)
                return false;

            entity.Icon = request.Icon;
            entity.Name = request.Name;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/part-categories/{id:int}", async (int id, UpdatePartCategoryBody body, ISender sender) =>
        {
            var command = new UpdatePartCategoryCommand(id, body.Icon, body.Name);
            var success = await sender.Send(command);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdatePartCategory")
        .WithTags("PartCategories");
    }

    public record UpdatePartCategoryBody(string Icon, string Name);

    public class Validator : AbstractValidator<UpdatePartCategoryCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Icon).NotEmpty().MaximumLength(20);
        }
    }
}