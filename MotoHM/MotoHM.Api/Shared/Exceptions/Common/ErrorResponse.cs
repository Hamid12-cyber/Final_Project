namespace MotoHM.Api.Shared.Exceptions.Common;

public record ErrorResponse(string? ErrorCode, int StatusCode, string Message);