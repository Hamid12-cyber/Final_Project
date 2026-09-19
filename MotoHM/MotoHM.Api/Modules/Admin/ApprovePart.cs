using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Exceptions.Common;

namespace MotoHM.Api.Modules.Admin;

public static class ApprovePart
{
    public record Command(int Id) : IRequest;

    public class Handler : IRequestHandler<Command>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var entity = await _db.Parts.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (entity is null)
                throw new AppException("NOT_FOUND", StatusCodes.Status404NotFound,
                    $"Part (Id={request.Id}) tapılmadı.");

            entity.Status = ApprovalStatus.Approved;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/admin/parts/{id:int}/approve", async (int id, ISender sender) =>
        {
            await sender.Send(new Command(id));
            return Results.NoContent();
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("ApprovePart")
        .WithTags("Admin");
    }
}