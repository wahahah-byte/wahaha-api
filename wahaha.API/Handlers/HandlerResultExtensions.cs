using Microsoft.AspNetCore.Mvc;

namespace wahaha.API.Handlers;

public static class HandlerResultExtensions
{
    // Translate HandlerResult into an ActionResult (callers needing CreatedAtAction handle Ok themselves).
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
