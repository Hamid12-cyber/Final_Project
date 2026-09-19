using Microsoft.AspNetCore.Diagnostics;
using MotoHM.Api.Shared.Exceptions.Common;

namespace MotoHM.Api.Shared.Handlers;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Gözlənilməz daxili xəta: {Path}", httpContext.Request.Path);

        ErrorResponse response = new("INTERNAL_SERVER_ERROR", 500, "Internal server error");

        httpContext.Response.StatusCode = 500;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}