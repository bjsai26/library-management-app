namespace LibraryManagement.AppServices.Common;

/// <summary>
/// Classification of a failed result. The API layer turns this into an HTTP status code.
/// </summary>
public enum ServiceError
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4
}

/// <summary>
/// Outcome of a service call. Services do not throw for expected failures such as
/// "not found" or "ISBN already used" - they return a failed result instead.
/// </summary>
public class ServiceResult
{
    public bool Succeeded { get; protected init; }

    public string Message { get; protected init; } = string.Empty;

    public ServiceError Error { get; protected init; } = ServiceError.None;

    public static ServiceResult Success(string message = "Request processed successfully.")
        => new() { Succeeded = true, Message = message };

    /// <summary>A failed result. Defaults to Validation, which the API returns as 400.</summary>
    public static ServiceResult Fail(string message, ServiceError error = ServiceError.Validation)
        => new() { Succeeded = false, Message = message, Error = error };

    public static ServiceResult NotFound(string message) => Fail(message, ServiceError.NotFound);

    public static ServiceResult Conflict(string message) => Fail(message, ServiceError.Conflict);
}

/// <inheritdoc cref="ServiceResult" />
public sealed class ServiceResult<T> : ServiceResult
{
    public T? Data { get; private init; }

    public static ServiceResult<T> Success(T data, string message = "Request processed successfully.")
        => new() { Succeeded = true, Data = data, Message = message };

    public static new ServiceResult<T> Fail(string message, ServiceError error = ServiceError.Validation)
        => new() { Succeeded = false, Message = message, Error = error };

    public static new ServiceResult<T> NotFound(string message) => Fail(message, ServiceError.NotFound);

    public static new ServiceResult<T> Conflict(string message) => Fail(message, ServiceError.Conflict);

    public static ServiceResult<T> Unauthorized(string message) => Fail(message, ServiceError.Unauthorized);
}
