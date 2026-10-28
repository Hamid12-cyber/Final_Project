using MotoHM.Api.Entites.Enums;

namespace MotoHM.Api.Modules.Admin;

public static class RejectPart
{
    public record Command(int Id, string Reason) : IRequest;

    public record RejectBody(string Reason);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Reason).NotEmpty().WithMessage("Rədd səbəbi yazılmalıdır.")
                .MaximumLength(500).WithMessage("Rədd səbəbi 500 simvoldan uzun ola bilməz.");
        }
    }

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

            entity.Status = ApprovalStatus.Rejected;
            entity.RejectionReason = request.Reason.Trim();
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/admin/parts/{id:int}/reject", async (int id, RejectBody body, ISender sender) =>
        {
            await sender.Send(new Command(id, body.Reason));
            return Results.NoContent();
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("RejectPart")
        .WithTags("Admin");
    }
}