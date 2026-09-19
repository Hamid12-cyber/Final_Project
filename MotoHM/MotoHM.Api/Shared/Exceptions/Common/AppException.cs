namespace MotoHM.Api.Shared.Exceptions.Common;

public sealed class AppException : ApplicationException
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    public AppException(string errorCode, int statusCode, string message) : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}