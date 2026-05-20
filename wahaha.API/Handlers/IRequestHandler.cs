namespace wahaha.API.Handlers;

// One handler per endpoint; controllers feed in Request DTOs and translate HandlerResult to ActionResult.
public interface IRequestHandler<in TRequest, TResponse>
{
    Task<HandlerResult<TResponse>> HandleAsync(TRequest request, CancellationToken ct = default);
}
