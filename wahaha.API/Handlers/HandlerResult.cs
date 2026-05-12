namespace wahaha.API.Handlers;

public enum HandlerStatus
{
    Ok,
    NoContent,
    NotFound,
    BadRequest,
    Unauthorized,
    Conflict,
}

/// <summary>
/// Transport-agnostic result a handler returns to its controller. The
/// controller is responsible for translating this into an MVC
/// <c>ActionResult</c> via <see cref="HandlerResultExtensions"/>.
/// </summary>
public sealed class HandlerResult<T>
{
    public HandlerStatus Status { get; }
    public T? Value { get; }
    public string? Error { get; }

    private HandlerResult(HandlerStatus status, T? value, string? error)
    {
        Status = status;
        Value = value;
        Error = error;
    }

    public static HandlerResult<T> Ok(T value) => new(HandlerStatus.Ok, value, null);
    public static HandlerResult<T> NoContent() => new(HandlerStatus.NoContent, default, null);
    public static HandlerResult<T> NotFound(string error) => new(HandlerStatus.NotFound, default, error);
    public static HandlerResult<T> BadRequest(string error) => new(HandlerStatus.BadRequest, default, error);
    public static HandlerResult<T> Unauthorized(string error) => new(HandlerStatus.Unauthorized, default, error);
    public static HandlerResult<T> Conflict(string error) => new(HandlerStatus.Conflict, default, error);
}

/// <summary>
/// Stand-in payload type for handlers that don't return data (PATCH/DELETE,
/// usually paired with <see cref="HandlerStatus.NoContent"/>).
/// </summary>
public sealed class Unit
{
    public static readonly Unit Value = new();
    private Unit() { }
}
