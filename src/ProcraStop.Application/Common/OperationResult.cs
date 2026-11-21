namespace ProcraStop.Application.Common;

public enum OperationError
{
    None,
    ValidationError,
    NotFound,
    NotificationPermissionMissing,
    Conflict,
    Unexpected
}

public record OperationResult(bool IsSuccess, OperationError Error, string? Message)
{
    public static OperationResult Success() => new(true, OperationError.None, null);
    public static OperationResult Failure(OperationError error, string message) => new(false, error, message);
}

public sealed record OperationResult<T>(bool IsSuccess, T? Value, OperationError Error, string? Message)
{
    public static OperationResult<T> Success(T value) => new(true, value, OperationError.None, null);
    public static OperationResult<T> Failure(OperationError error, string message) => new(false, default, error, message);
}

public static class OperationResultMapper
{
    public static OperationResult From(Exception exception) => exception switch
    {
        ArgumentException => OperationResult.Failure(OperationError.ValidationError, exception.Message),
        KeyNotFoundException => OperationResult.Failure(OperationError.NotFound, exception.Message),
        InvalidOperationException => OperationResult.Failure(OperationError.Conflict, exception.Message),
        _ => OperationResult.Failure(OperationError.Unexpected, "No se pudo completar la operación.")
    };

    public static OperationResult<T> From<T>(Exception exception) => exception switch
    {
        ArgumentException => OperationResult<T>.Failure(OperationError.ValidationError, exception.Message),
        KeyNotFoundException => OperationResult<T>.Failure(OperationError.NotFound, exception.Message),
        InvalidOperationException => OperationResult<T>.Failure(OperationError.Conflict, exception.Message),
        _ => OperationResult<T>.Failure(OperationError.Unexpected, "No se pudo completar la operación.")
    };
}
