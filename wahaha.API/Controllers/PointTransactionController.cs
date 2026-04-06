using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PointTransactionsController : ControllerBase
{
    private readonly IPointTransactionRepository _pointTransactionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PointTransactionsController> _logger;

    public PointTransactionsController(
        IPointTransactionRepository pointTransactionRepository,
        IMapper mapper,
        ILogger<PointTransactionsController> logger)
    {
        _pointTransactionRepository = pointTransactionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PointTransactionDto>>> GetAll([FromQuery] PointTransactionFilterParams filters)
    {
        var userId = GetCurrentUserId();
        filters.UserId = userId;
        _logger.LogDebug("Fetching point transactions for user {UserId}", userId);

        var result = await _pointTransactionRepository.GetFilteredAsync(filters);

        return Ok(new PagedResult<PointTransactionDto>
        {
            Data = _mapper.Map<IEnumerable<PointTransactionDto>>(result.Data),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PointTransactionDto>> GetById(int id)
    {
        _logger.LogDebug("Fetching transaction {TransactionId}", id);
        var transaction = await _pointTransactionRepository.GetByIdAsync(id);

        if (transaction == null || transaction.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Transaction {TransactionId} not found or unauthorized", id);
            return NotFound($"Transaction with ID {id} was not found.");
        }

        return Ok(_mapper.Map<PointTransactionDto>(transaction));
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<PointTransactionDto>>> GetByType(TransactionType type)
    {
        var userId = GetCurrentUserId();
        _logger.LogDebug("Fetching {Type} transactions for user {UserId}", type, userId);

        var transactions = await _pointTransactionRepository.GetByUserAndTypeAsync(userId, type);
        return Ok(_mapper.Map<IEnumerable<PointTransactionDto>>(transactions));
    }

    [HttpPost]
    public async Task<ActionResult<PointTransactionDto>> Create(CreatePointTransactionDto dto)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Creating {Type} transaction of {Amount} points for user {UserId}",
            dto.Type, dto.Amount, userId);

        var transaction = _mapper.Map<PointTransaction>(dto);
        transaction.UserId = userId;
        var created = await _pointTransactionRepository.CreateAsync(transaction);

        _logger.LogInformation("Transaction {TransactionId} created for user {UserId}",
            created.TransactionId, userId);

        return CreatedAtAction(nameof(GetById), new { id = created.TransactionId },
            _mapper.Map<PointTransactionDto>(created));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting transaction {TransactionId}", id);
        var transaction = await _pointTransactionRepository.GetByIdAsync(id);

        if (transaction == null || transaction.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Transaction {TransactionId} not found or unauthorized for deletion", id);
            return NotFound($"Transaction with ID {id} was not found.");
        }

        await _pointTransactionRepository.DeleteAsync(id);
        _logger.LogInformation("Transaction {TransactionId} deleted successfully", id);
        return NoContent();
    }
}
