using Microsoft.AspNetCore.Diagnostics;
using MotoHM.Api.Shared.Exceptions.Common;

namespace MotoHM.Api.Shared.Handlers;

public sealed class AppExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not AppException ex)
        {
            return false;
        }

        ErrorResponse response = new(ex.ErrorCode, ex.StatusCode, ex.Message);

        httpContext.Response.StatusCode = ex.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}