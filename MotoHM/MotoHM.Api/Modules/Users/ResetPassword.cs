using Microsoft.AspNetCore.RateLimiting;
using MotoHM.Api.Shared.Security;

namespace MotoHM.Api.Modules.Users;

public static class ResetPassword
{
    public record ResetPasswordCommand(string Email, string Code, string NewPassword) : IRequest<Unit>;

    public class Handler : IRequestHandler<ResetPasswordCommand, Unit>
    {
        private readonly IAppDbContext _db;
        private readonly IConfiguration _config;

        public Handler(IAppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim();
            var key = "reset:" + email.ToLowerInvariant();

            if (CodeAttemptLimiter.IsLocked(key))
                throw new AppException("TOO_MANY_ATTEMPTS", StatusCodes.Status429TooManyRequests,
                    "Çox sayda yanlış cəhd oldu. Yeni kod istəyin və ya 15 dəqiqə sonra yenidən cəhd edin.");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            var secret = _config["Jwt:Key"] ?? "motohm-dev-secret";

            var valid = user is not null
                && !string.IsNullOrEmpty(user.PasswordResetToken)
                && user.PasswordResetTokenExpiry >= DateTime.UtcNow
                && CodeHasher.Matches(user.Email, request.Code, secret, user.PasswordResetToken);

            if (!valid)
            {
                CodeAttemptLimiter.RegisterFailure(key);
                throw new AppException("INVALID_CODE", StatusCodes.Status400BadRequest,
                    "Kod yanlışdır və ya vaxtı bitib.");
            }

            user!.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = default;
            await _db.SaveChangesAsync(cancellationToken);

            CodeAttemptLimiter.Reset(key);
            return Unit.Value;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/reset-password", async (ResetPasswordCommand command, ISender sender) =>
        {
            await sender.Send(command);
            return Results.Ok(new { message = "Şifrə uğurla yeniləndi." });
        })
        .WithName("ResetPassword")
        .WithTags("Auth")
        .RequireRateLimiting("auth");
    }

    public class Validator : AbstractValidator<ResetPasswordCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Code).NotEmpty().Matches("^[0-9]{6}$").WithMessage("Kod 6 rəqəmdən ibarət olmalıdır.");
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
        }
    }
}