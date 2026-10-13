using System.Security.Claims;
using MotoHM.Api.Shared.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;

namespace MotoHM.Api.Modules.Parts;

public static class DeletePart
{
    public record DeletePartCommand(int Id, int UserId, bool IsAdmin) : IRequest<bool>;

    public class Handler : IRequestHandler<DeletePartCommand, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;
        public async Task<bool> Handle(DeletePartCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.Parts.FirstOrDefaultAsync(p => p.Id == request.Id && (request.IsAdmin || p.SellerId == request.UserId), cancellationToken);

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
        app.MapDelete("/api/parts/{id:int}", async (int id, ClaimsPrincipal user, ISender sender) =>
        {
            var success = await sender.Send(new DeletePartCommand(id, user.GetUserId(), user.IsInRole("Admin")));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("DeletePart")
        .WithTags("Parts");
    }
}