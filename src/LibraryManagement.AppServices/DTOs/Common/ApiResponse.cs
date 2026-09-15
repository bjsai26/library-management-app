namespace LibraryManagement.AppServices.DTOs.Common;

/// <summary>
/// The envelope every endpoint returns, so clients see one shape for success and failure.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public T? Data { get; set; }

    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

    public static ApiResponse<T> Ok(T data, string message) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };
}

/// <summary>Envelope for endpoints that return no payload.</summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message) => new() { Success = true, Message = message };

    public static ApiResponse Fail(string message, IReadOnlyList<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors ?? Array.Empty<string>()
    };
}
