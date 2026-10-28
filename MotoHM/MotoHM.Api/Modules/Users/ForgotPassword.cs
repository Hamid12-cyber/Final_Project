using Microsoft.AspNetCore.RateLimiting;
using MotoHM.Api.Shared.Security;

namespace MotoHM.Api.Modules.Users;

public static class ForgotPassword
{
    private const int CodeValidMinutes = 10;

    public record ForgotPasswordCommand(string Email, string? Lang = null) : IRequest<Unit>;

    public class Handler : IRequestHandler<ForgotPasswordCommand, Unit>
    {
        private readonly IAppDbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;
        private readonly ILogger<Handler> _logger;

        public Handler(IAppDbContext db, IEmailSender emailSender, IConfiguration config, ILogger<Handler> logger)
        {
            _db = db;
            _emailSender = emailSender;
            _config = config;
            _logger = logger;
        }

        public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            // Təhlükəsizlik: email tapılmasa da eyni cavab qaytarılır.
            if (user is null)
                return Unit.Value;

            var secret = _config["Jwt:Key"] ?? "motohm-dev-secret";
            var code = CodeHasher.NewCode();

            user.PasswordResetToken = CodeHasher.Hash(user.Email, code, secret);
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(CodeValidMinutes);
            await _db.SaveChangesAsync(cancellationToken);

            CodeAttemptLimiter.Reset("reset:" + email.ToLowerInvariant());

            try
            {
                var mail = EmailTemplates.Code(EmailPurpose.ResetPassword, request.Lang, user.FullName, code, CodeValidMinutes);
                await _emailSender.SendAsync(user.Email, mail.Subject, mail.Body, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifrə bərpa kodu göndərilmədi: {Email}", user.Email);
            }

            return Unit.Value;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/forgot-password", async (ForgotPasswordCommand command, HttpRequest httpRequest, ISender sender) =>
        {
            await sender.Send(command with { Lang = EmailLanguage.FromRequest(httpRequest) });
            return Results.Ok(new { message = "Əgər bu email qeydiyyatdan keçibsə, təsdiq kodu göndərildi." });
        })
        .WithName("ForgotPassword")
        .WithTags("Auth")
        .RequireRateLimiting("auth");
    }

    public class Validator : AbstractValidator<ForgotPasswordCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }
}