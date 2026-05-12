namespace wahaha.API.Handlers;

/// <summary>
/// One-class-per-endpoint pattern. Controllers receive a Request DTO from the
/// HTTP pipeline, hand it to the matching handler, then translate the handler's
/// <see cref="HandlerResult{T}"/> back into an MVC <c>ActionResult</c>.
/// </summary>
/// <typeparam name="TRequest">Endpoint input — usually a record with the route/body params plus the calling user id.</typeparam>
/// <typeparam name="TResponse">Endpoint payload type. Use <see cref="Unit"/> for commands that return no body.</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
{
    Task<HandlerResult<TResponse>> HandleAsync(TRequest request, CancellationToken ct = default);
}
