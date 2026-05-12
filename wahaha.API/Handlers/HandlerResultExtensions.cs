using Microsoft.AspNetCore.Mvc;

namespace wahaha.API.Handlers;

public static class HandlerResultExtensions
{
    /// <summary>
    /// Translates a <see cref="HandlerResult{T}"/> into an MVC
    /// <c>ActionResult</c>. Callers that need <c>CreatedAtAction</c> (with a
    /// Location header) should handle the OK case themselves and only fall
    /// through to this for the error branches.
    /// </summary>
    public static ActionResult ToActionResult<T>(this HandlerResult<T> result)
        => result.Status switch
        {
            HandlerStatus.Ok => new OkObjectResult(result.Value),
            HandlerStatus.NoContent => new NoContentResult(),
            HandlerStatus.NotFound => new NotFoundObjectResult(result.Error),
            HandlerStatus.BadRequest => new BadRequestObjectResult(result.Error),
            HandlerStatus.Unauthorized => new UnauthorizedObjectResult(result.Error),
            HandlerStatus.Conflict => new ConflictObjectResult(result.Error),
            _ => new ObjectResult(result.Error) { StatusCode = 500 },
        };
}
