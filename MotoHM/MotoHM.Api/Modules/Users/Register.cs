using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MotoHM.Api.Entites;
using MotoHM.Api.Entites.Enums;
using MotoHM.Api.Shared.Exceptions.Common;

namespace MotoHM.Api.Modules.Users;

public static class Register
{
    public record RegisterCommand(string FullName, string Email, string Password, UserRole Role) : IRequest<int>;

    public class Handler : IRequestHandler<RegisterCommand, int>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<int> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var emailTaken = await _db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
            if (emailTaken)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Bu email artıq qeydiyyatdan keçib.");

            var entity = new UserEntity
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role
            };

            _db.Users.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (RegisterCommand command, ISender sender) =>
        {
            var id = await sender.Send(command);
            return Results.Created($"/api/users/{id}", new { id });
        })
        .WithName("Register")
        .WithTags("Auth");
    }
    public class Validator : AbstractValidator<RegisterCommand>
    {
        public Validator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        }
    }
}