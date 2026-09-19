using System.Security.Claims;
using MotoHM.Api.Shared.Exceptions.Common;

namespace MotoHM.Api.Shared.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        if (!int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            throw new AppException("UNAUTHORIZED", StatusCodes.Status401Unauthorized, "Token etibarsızdır.");

        return userId;
    }
}