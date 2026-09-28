using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Shared.Exceptions.Common;

namespace MotoHM.Api.Modules.Users;

public static class ResetPassword
{
    public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<Unit>;

    public class Handler : IRequestHandler<ResetPasswordCommand, Unit>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == request.Token, cancellationToken);

            if (user is null || user.PasswordResetTokenExpiry is null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Bərpa linki etibarsızdır və ya vaxtı bitib.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _db.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/reset-password", async (ResetPasswordCommand command, ISender sender) =>
        {
            await sender.Send(command);
            return Results.Ok(new { message = "Şifrəniz uğurla yeniləndi." });
        })
        .WithName("ResetPassword")
        .WithTags("Auth");
    }

    public class Validator : AbstractValidator<ResetPasswordCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
        }
    }
}