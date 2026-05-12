using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Handlers;
using wahaha.API.Handlers.Points;
using wahaha.API.Models.DTO;

namespace wahaha.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PointsController : ControllerBase
{
    private readonly IRequestHandler<SubmitPointsHandlerRequest, SubmitPointsResponse> _submit;

    public PointsController(IRequestHandler<SubmitPointsHandlerRequest, SubmitPointsResponse> submit)
    {
        _submit = submit;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpPost("submit")]
    public async Task<ActionResult<SubmitPointsResponse>> Submit([FromBody] SubmitPointsRequest request)
        => (await _submit.HandleAsync(new SubmitPointsHandlerRequest(GetCurrentUserId(), request))).ToActionResult();
}
