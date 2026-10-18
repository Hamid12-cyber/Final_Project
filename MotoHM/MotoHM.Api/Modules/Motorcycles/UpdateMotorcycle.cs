using System.Security.Claims;
using MotoHM.Api.Shared.Extensions;
using FluentValidation;

namespace MotoHM.Api.Modules.Motorcycles;

public static class UpdateMotorcycle
{
    public record UpdateMotorcycleCommand(int Id, string Name, string Brand, string Model, int Cc, int Year,
    decimal Price, string? ImageUrl, bool IsForRent, bool IsForSale,
    int UserId, bool IsAdmin) : IRequest<bool>;

    public class Handler : IRequestHandler<UpdateMotorcycleCommand, bool>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<bool> Handle(UpdateMotorcycleCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.Motorcycles.FirstOrDefaultAsync(m => m.Id == request.Id && (request.IsAdmin || m.SellerId == request.UserId), cancellationToken);

            if (entity is null)
                return false;

            entity.Name = request.Name;
            entity.Brand = request.Brand;
            entity.Model = request.Model;
            entity.Cc = request.Cc;
            entity.Year = request.Year;
            entity.Price = request.Price;
            entity.ImageUrl = request.ImageUrl;
            entity.IsForRent = request.IsForRent;
            entity.IsForSale = request.IsForSale;
            // Satıcı elanı dəyişəndə (rədd edilmiş də olsa, təsdiqlənmiş də) yenidən admin təsdiqinə göndərilir
            if (!request.IsAdmin)
            {
                entity.Status = MotoHM.Api.Entites.Enums.ApprovalStatus.Pending;
                entity.RejectionReason = null;
            }

            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/motorcycles/{id:int}", async (int id, UpdateMotorcycleBody body, ClaimsPrincipal user, ISender sender) =>
        {
            var command = new UpdateMotorcycleCommand(id, body.Name, body.Brand, body.Model, body.Cc, body.Year,
                body.Price, body.ImageUrl, body.IsForRent, body.IsForSale,
                user.GetUserId(), user.IsInRole("Admin"));

            var success = await sender.Send(command);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("UpdateMotorcycle")
        .WithTags("Motorcycles");
    }

    public record UpdateMotorcycleBody(string Name, string Brand, string Model, int Cc, int Year,
        decimal Price, string? ImageUrl, bool IsForRent, bool IsForSale);

    public class Validator : AbstractValidator<UpdateMotorcycleCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Brand).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Model).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Cc).GreaterThan(0);
            RuleFor(x => x.Year).InclusiveBetween(1980, DateTime.UtcNow.Year + 1);
            RuleFor(x => x.Price).GreaterThan(0);
        }
    }
}