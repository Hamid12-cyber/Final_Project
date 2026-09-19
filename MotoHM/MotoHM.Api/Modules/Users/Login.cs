using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MotoHM.Api.Data;
using MotoHM.Api.Shared.Exceptions.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MotoHM.Api.Modules.Users;

public static class Login
{
    public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;

    public record LoginResponse(string Token, string FullName, string Role);

    public class Handler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IAppDbContext _db;
        private readonly IConfiguration _configuration;

        public Handler(IAppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new AppException("UNAUTHORIZED", StatusCodes.Status401Unauthorized,
                    "Email və ya şifrə səhvdir.");

            var token = GenerateToken(user.Id, user.Email, user.Role.ToString());

            return new LoginResponse(token, user.FullName, user.Role.ToString());
        }

        private string GenerateToken(int userId, string email, string role)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings["Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]!)),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (LoginCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("Login")
        .WithTags("Auth");
    }
}