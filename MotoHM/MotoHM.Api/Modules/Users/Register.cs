using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Security;

namespace MotoHM.Api.Modules.Users;

public static class Register
{
    private const int CodeValidMinutes = 10;
    private const int SessionValidHours = 1;
    private const string ProtectorPurpose = "MotoHM.PendingRegistration.v1";

    private sealed record PendingRegistration(
        string FullName,
        string Email,
        string PasswordHash,
        UserRole Role,
        string CodeHash,
        DateTime CodeExpiresUtc,
        DateTime IssuedUtc);

    public record RegisterCommand(string FullName, string Email, string Password, UserRole Role, string? Lang = null)
        : IRequest<RegisterResult>;

    public record RegisterResult(string VerificationToken, string Email, int ExpiresInMinutes);

    public record VerifyRegistrationCommand(string VerificationToken, string Code) : IRequest<int>;

    public record ResendRegistrationCodeCommand(string VerificationToken, string? Lang = null)
        : IRequest<RegisterResult>;

    // ---------- köməkçi metodlar ----------

    private static string Secret(IConfiguration config) => config["Jwt:Key"] ?? "motohm-dev-secret";

    private static string LimiterKey(string email) => "register:" + email.Trim().ToLowerInvariant();

    private static string Protect(IDataProtectionProvider dp, PendingRegistration pending) =>
        dp.CreateProtector(ProtectorPurpose).Protect(JsonSerializer.Serialize(pending));

    private static PendingRegistration? Unprotect(IDataProtectionProvider dp, string token)
    {
        try
        {
            var json = dp.CreateProtector(ProtectorPurpose).Unprotect(token);
            return JsonSerializer.Deserialize<PendingRegistration>(json);
        }
        catch
        {
            return null;
        }
    }

    private static async Task SendCodeAsync(
        IEmailSender sender, ILogger logger, string? lang, string email, string fullName, string code, CancellationToken ct)
    {
        try
        {
            var mail = EmailTemplates.Code(EmailPurpose.VerifyEmail, lang, fullName, code, CodeValidMinutes);
            await sender.SendAsync(email, mail.Subject, mail.Body, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Təsdiq kodu emaili göndərilmədi: {Email}", email);
            throw new AppException("EMAIL_SEND_FAILED", StatusCodes.Status503ServiceUnavailable,
                "Təsdiq kodu göndərilə bilmədi. Bir az sonra yenidən cəhd edin.");
        }
    }

    // ---------- 1) Qeydiyyat: kodu göndər ----------

    public class Handler : IRequestHandler<RegisterCommand, RegisterResult>
    {
        private readonly IAppDbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly IDataProtectionProvider _dp;
        private readonly IConfiguration _config;
        private readonly ILogger<Handler> _logger;

        public Handler(IAppDbContext db, IEmailSender emailSender, IDataProtectionProvider dp,
            IConfiguration config, ILogger<Handler> logger)
        {
            _db = db;
            _emailSender = emailSender;
            _dp = dp;
            _config = config;
            _logger = logger;
        }

        public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim();

            var emailTaken = await _db.Users.AnyAsync(u => u.Email == email, cancellationToken);
            if (emailTaken)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Bu email artıq qeydiyyatdan keçib.");

            var code = CodeHasher.NewCode();
            var now = DateTime.UtcNow;

            var pending = new PendingRegistration(
                request.FullName.Trim(),
                email,
                BCrypt.Net.BCrypt.HashPassword(request.Password),
                request.Role,
                CodeHasher.Hash(email, code, Secret(_config)),
                now.AddMinutes(CodeValidMinutes),
                now);

            await SendCodeAsync(_emailSender, _logger, request.Lang, email, pending.FullName, code, cancellationToken);

            CodeAttemptLimiter.Reset(LimiterKey(email));
            return new RegisterResult(Protect(_dp, pending), email, CodeValidMinutes);
        }
    }

    // ---------- 2) Kodu yoxla və hesabı yarat ----------

    public class VerifyHandler : IRequestHandler<VerifyRegistrationCommand, int>
    {
        private readonly IAppDbContext _db;
        private readonly IDataProtectionProvider _dp;
        private readonly IConfiguration _config;

        public VerifyHandler(IAppDbContext db, IDataProtectionProvider dp, IConfiguration config)
        {
            _db = db;
            _dp = dp;
            _config = config;
        }

        public async Task<int> Handle(VerifyRegistrationCommand request, CancellationToken cancellationToken)
        {
            var pending = Unprotect(_dp, request.VerificationToken)
                ?? throw new AppException("INVALID_TOKEN", StatusCodes.Status400BadRequest,
                    "Təsdiq sessiyası etibarsızdır. Yenidən qeydiyyatdan keçin.");

            var key = LimiterKey(pending.Email);

            if (CodeAttemptLimiter.IsLocked(key))
                throw new AppException("TOO_MANY_ATTEMPTS", StatusCodes.Status429TooManyRequests,
                    "Çox sayda yanlış cəhd oldu. Yeni kod istəyin və ya 15 dəqiqə sonra yenidən cəhd edin.");

            if (pending.CodeExpiresUtc < DateTime.UtcNow)
                throw new AppException("CODE_EXPIRED", StatusCodes.Status400BadRequest,
                    "Kodun vaxtı bitib. Yeni kod istəyin.");

            if (!CodeHasher.Matches(pending.Email, request.Code, Secret(_config), pending.CodeHash))
            {
                CodeAttemptLimiter.RegisterFailure(key);
                throw new AppException("INVALID_CODE", StatusCodes.Status400BadRequest, "Kod yanlışdır.");
            }

            var emailTaken = await _db.Users.AnyAsync(u => u.Email == pending.Email, cancellationToken);
            if (emailTaken)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Bu email artıq qeydiyyatdan keçib.");

            var entity = new UserEntity
            {
                FullName = pending.FullName,
                Email = pending.Email,
                PasswordHash = pending.PasswordHash,
                Role = pending.Role
            };

            _db.Users.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            CodeAttemptLimiter.Reset(key);
            return entity.Id;
        }
    }

    // ---------- 3) Kodu yenidən göndər ----------

    public class ResendHandler : IRequestHandler<ResendRegistrationCodeCommand, RegisterResult>
    {
        private readonly IEmailSender _emailSender;
        private readonly IDataProtectionProvider _dp;
        private readonly IConfiguration _config;
        private readonly ILogger<ResendHandler> _logger;

        public ResendHandler(IEmailSender emailSender, IDataProtectionProvider dp,
            IConfiguration config, ILogger<ResendHandler> logger)
        {
            _emailSender = emailSender;
            _dp = dp;
            _config = config;
            _logger = logger;
        }

        public async Task<RegisterResult> Handle(ResendRegistrationCodeCommand request, CancellationToken cancellationToken)
        {
            var old = Unprotect(_dp, request.VerificationToken);
            if (old is null || old.IssuedUtc.AddHours(SessionValidHours) < DateTime.UtcNow)
                throw new AppException("INVALID_TOKEN", StatusCodes.Status400BadRequest,
                    "Təsdiq sessiyasının vaxtı bitib. Yenidən qeydiyyatdan keçin.");

            var code = CodeHasher.NewCode();
            var pending = old with
            {
                CodeHash = CodeHasher.Hash(old.Email, code, Secret(_config)),
                CodeExpiresUtc = DateTime.UtcNow.AddMinutes(CodeValidMinutes)
            };

            await SendCodeAsync(_emailSender, _logger, request.Lang, pending.Email, pending.FullName, code, cancellationToken);

            CodeAttemptLimiter.Reset(LimiterKey(pending.Email));
            return new RegisterResult(Protect(_dp, pending), pending.Email, CodeValidMinutes);
        }
    }

    // ---------- endpoint-lər ----------

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (RegisterCommand command, HttpRequest httpRequest, ISender sender) =>
        {
            var result = await sender.Send(command with { Lang = EmailLanguage.FromRequest(httpRequest) });
            return Results.Ok(result);
        })
        .WithName("Register")
        .WithTags("Auth")
        .RequireRateLimiting("auth");

        app.MapPost("/api/auth/verify-registration", async (VerifyRegistrationCommand command, ISender sender) =>
        {
            var id = await sender.Send(command);
            return Results.Created($"/api/users/{id}", new { id });
        })
        .WithName("VerifyRegistration")
        .WithTags("Auth")
        .RequireRateLimiting("auth");

        app.MapPost("/api/auth/resend-registration-code", async (ResendRegistrationCodeCommand command, HttpRequest httpRequest, ISender sender) =>
        {
            var result = await sender.Send(command with { Lang = EmailLanguage.FromRequest(httpRequest) });
            return Results.Ok(result);
        })
        .WithName("ResendRegistrationCode")
        .WithTags("Auth")
        .RequireRateLimiting("auth");
    }

    // ---------- validatorlar ----------

    public class Validator : AbstractValidator<RegisterCommand>
    {
        public Validator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
            RuleFor(x => x.Role)
                .Must(r => r == UserRole.Customer || r == UserRole.Seller)
                .WithMessage("Yanlış hesab növü.");
        }
    }

    public class VerifyValidator : AbstractValidator<VerifyRegistrationCommand>
    {
        public VerifyValidator()
        {
            RuleFor(x => x.VerificationToken).NotEmpty();
            RuleFor(x => x.Code).NotEmpty().Length(6).Matches("^[0-9]{6}$")
                .WithMessage("Kod 6 rəqəmdən ibarət olmalıdır.");
        }
    }

    public class ResendValidator : AbstractValidator<ResendRegistrationCodeCommand>
    {
        public ResendValidator()
        {
            RuleFor(x => x.VerificationToken).NotEmpty();
        }
    }
}