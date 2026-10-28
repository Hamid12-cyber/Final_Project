using MotoHM.Api.Shared.Extensions;

namespace MotoHM.Api.Modules.Users;

public static class UpdateAvatar
{
    public record UpdateAvatarCommand(int UserId, string? AvatarUrl) : IRequest<string?>;

    public class Handler : IRequestHandler<UpdateAvatarCommand, string?>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<string?> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user is null)
                throw new AppException("NOT_FOUND", StatusCodes.Status404NotFound, "İstifadəçi tapılmadı.");

            // Boş göndərilirsə şəkil silinir və frontend yenidən default avatar göstərir
            user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return user.AvatarUrl;
        }
    }

    public record UpdateAvatarBody(string? AvatarUrl);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/users/me/avatar", async (UpdateAvatarBody body, ClaimsPrincipal user, ISender sender) =>
        {
            var avatarUrl = await sender.Send(new UpdateAvatarCommand(user.GetUserId(), body.AvatarUrl));
            return Results.Ok(new { avatarUrl });
        })
        .RequireAuthorization()
        .WithName("UpdateAvatar")
        .WithTags("Users");
    }

    public class Validator : AbstractValidator<UpdateAvatarCommand>
    {
        public Validator()
        {
            RuleFor(x => x.AvatarUrl)
                .MaximumLength(500)
                .Must(BeOwnUpload)
                .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl))
                .WithMessage("Profil şəkli yalnız sayta yüklənmiş şəkil ola bilər.");
        }

        // /api/uploads/images-in qaytardığı ünvan (…/uploads/<fayl>) olmalıdır — başqa saytın linki yox
        private static bool BeOwnUpload(string? url) =>
            Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && uri.AbsolutePath.StartsWith("/uploads/", StringComparison.Ordinal);
    }
}