using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Handlers;
using wahaha.API.Handlers.PointTransactions;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;

namespace wahaha.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PointTransactionsController : ControllerBase
{
    private readonly IRequestHandler<GetTransactionListRequest, PagedResult<PointTransactionDto>> _list;
    private readonly IRequestHandler<GetTransactionByIdRequest, PointTransactionDto> _byId;
    private readonly IRequestHandler<GetTransactionsByTypeRequest, IEnumerable<PointTransactionDto>> _byType;
    private readonly IRequestHandler<CreateTransactionRequest, PointTransactionDto> _create;
    private readonly IRequestHandler<DeleteTransactionRequest, Unit> _delete;

    public PointTransactionsController(
        IRequestHandler<GetTransactionListRequest, PagedResult<PointTransactionDto>> list,
        IRequestHandler<GetTransactionByIdRequest, PointTransactionDto> byId,
        IRequestHandler<GetTransactionsByTypeRequest, IEnumerable<PointTransactionDto>> byType,
        IRequestHandler<CreateTransactionRequest, PointTransactionDto> create,
        IRequestHandler<DeleteTransactionRequest, Unit> delete)
    {
        _list = list;
        _byId = byId;
        _byType = byType;
        _create = create;
        _delete = delete;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PointTransactionDto>>> GetAll([FromQuery] PointTransactionFilterParams filters)
        => (await _list.HandleAsync(new GetTransactionListRequest(GetCurrentUserId(), filters))).ToActionResult();

    [HttpGet("{id}")]
    public async Task<ActionResult<PointTransactionDto>> GetById(int id)
        => (await _byId.HandleAsync(new GetTransactionByIdRequest(id, GetCurrentUserId()))).ToActionResult();

    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<PointTransactionDto>>> GetByType(TransactionType type)
        => (await _byType.HandleAsync(new GetTransactionsByTypeRequest(GetCurrentUserId(), type))).ToActionResult();

    [HttpPost]
    public async Task<ActionResult<PointTransactionDto>> Create(CreatePointTransactionDto dto)
    {
        var result = await _create.HandleAsync(new CreateTransactionRequest(GetCurrentUserId(), dto));
        if (result.Status == HandlerStatus.Ok && result.Value != null)
            return CreatedAtAction(nameof(GetById), new { id = result.Value.TransactionId }, result.Value);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => (await _delete.HandleAsync(new DeleteTransactionRequest(id, GetCurrentUserId()))).ToActionResult();
}
