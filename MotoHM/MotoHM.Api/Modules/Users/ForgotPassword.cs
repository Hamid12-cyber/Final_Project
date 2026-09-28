using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Shared.Email;
using System.Security.Cryptography;

namespace MotoHM.Api.Modules.Users;

public static class ForgotPassword
{
    public record ForgotPasswordCommand(string Email) : IRequest<Unit>;

    public class Handler : IRequestHandler<ForgotPasswordCommand, Unit>
    {
        private readonly IAppDbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;

        public Handler(IAppDbContext db, IEmailSender emailSender, IConfiguration config)
        {
            _db = db;
            _emailSender = emailSender;
            _config = config;
        }

        public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            // Təhlükəsizlik üçün: email tapılmasa belə eyni cavabı qaytarırıq ki,
            // kənar şəxs bu endpoint ilə hansı email-lərin qeydiyyatdan keçdiyini yoxlaya bilməsin.
            if (user is null)
                return Unit.Value;

            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-").Replace("/", "_").Replace("=", "");

            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _db.SaveChangesAsync(cancellationToken);

            var frontendBaseUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var resetLink = $"{frontendBaseUrl}/reset-password?token={token}";

            var htmlBody = $"""
                <p>Salam {user.FullName},</p>
                <p>Şifrənizi bərpa etmək üçün <a href="{resetLink}">bu linkə</a> klikləyin.</p>
                <p>Link 1 saat ərzində etibarlıdır. Əgər bu tələbi siz göndərməmisinizsə, bu email-i nəzərə almayın.</p>
                """;

            await _emailSender.SendAsync(user.Email, "MotoHM — Şifrə bərpası", htmlBody, cancellationToken);

            return Unit.Value;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/forgot-password", async (ForgotPasswordCommand command, ISender sender) =>
        {
            await sender.Send(command);
            return Results.Ok(new { message = "Əgər bu email qeydiyyatdan keçibsə, şifrə bərpa linki göndərildi." });
        })
        .WithName("ForgotPassword")
        .WithTags("Auth");
    }

    public class Validator : AbstractValidator<ForgotPasswordCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }
}