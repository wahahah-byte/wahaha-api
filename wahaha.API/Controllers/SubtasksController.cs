using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Handlers;
using wahaha.API.Handlers.Subtasks;
using wahaha.API.Models.DTO;

namespace wahaha.API.Controllers;

[Authorize]
[ApiController]
public class SubtasksController : ControllerBase
{
    private readonly IRequestHandler<GetSubtasksForTaskRequest, IEnumerable<SubtaskDto>> _list;
    private readonly IRequestHandler<CreateSubtaskRequestModel, SubtaskDto> _create;
    private readonly IRequestHandler<UpdateSubtaskRequestModel, SubtaskDto> _update;
    private readonly IRequestHandler<DeleteSubtaskRequest, Unit> _delete;
    private readonly IRequestHandler<ReorderSubtasksRequestModel, Unit> _reorder;

    public SubtasksController(
        IRequestHandler<GetSubtasksForTaskRequest, IEnumerable<SubtaskDto>> list,
        IRequestHandler<CreateSubtaskRequestModel, SubtaskDto> create,
        IRequestHandler<UpdateSubtaskRequestModel, SubtaskDto> update,
        IRequestHandler<DeleteSubtaskRequest, Unit> delete,
        IRequestHandler<ReorderSubtasksRequestModel, Unit> reorder)
    {
        _list = list;
        _create = create;
        _update = update;
        _delete = delete;
        _reorder = reorder;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet("api/tasks/{taskId:guid}/subtasks")]
    public async Task<ActionResult<IEnumerable<SubtaskDto>>> GetForTask(Guid taskId)
        => (await _list.HandleAsync(new GetSubtasksForTaskRequest(taskId, GetCurrentUserId()))).ToActionResult();

    [HttpPost("api/tasks/{taskId:guid}/subtasks")]
    public async Task<ActionResult<SubtaskDto>> Create(Guid taskId, [FromBody] CreateSubtaskRequest request)
        => (await _create.HandleAsync(new CreateSubtaskRequestModel(taskId, GetCurrentUserId(), request))).ToActionResult();

    [HttpPatch("api/subtasks/{id:int}")]
    public async Task<ActionResult<SubtaskDto>> Update(int id, [FromBody] UpdateSubtaskRequest request)
        => (await _update.HandleAsync(new UpdateSubtaskRequestModel(id, GetCurrentUserId(), request))).ToActionResult();

    [HttpDelete("api/subtasks/{id:int}")]
    public async Task<ActionResult> Delete(int id)
        => (await _delete.HandleAsync(new DeleteSubtaskRequest(id, GetCurrentUserId()))).ToActionResult();

    [HttpPost("api/tasks/{taskId:guid}/subtasks/reorder")]
    public async Task<ActionResult> Reorder(Guid taskId, [FromBody] ReorderSubtasksRequest request)
        => (await _reorder.HandleAsync(new ReorderSubtasksRequestModel(taskId, GetCurrentUserId(), request))).ToActionResult();
}
