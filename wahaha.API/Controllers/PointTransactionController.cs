using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTOs;
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

    public PointTransactionsController(IPointTransactionRepository pointTransactionRepository, IMapper mapper)
    {
        _pointTransactionRepository = pointTransactionRepository;
        _mapper = mapper;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PointTransactionDto>>> GetAll([FromQuery] PointTransactionFilterParams filters)
    {
        filters.UserId = GetCurrentUserId();
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
        var transaction = await _pointTransactionRepository.GetByIdAsync(id);

        if (transaction == null || transaction.UserId != GetCurrentUserId())
            return NotFound($"Transaction with ID {id} was not found.");

        return Ok(_mapper.Map<PointTransactionDto>(transaction));
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<PointTransactionDto>>> GetByType(TransactionType type)
    {
        var transactions = await _pointTransactionRepository.GetByUserAndTypeAsync(GetCurrentUserId(), type);
        return Ok(_mapper.Map<IEnumerable<PointTransactionDto>>(transactions));
    }

    [HttpPost]
    public async Task<ActionResult<PointTransactionDto>> Create(CreatePointTransactionDto dto)
    {
        var transaction = _mapper.Map<PointTransaction>(dto);
        transaction.UserId = GetCurrentUserId();
        var created = await _pointTransactionRepository.CreateAsync(transaction);

        return CreatedAtAction(nameof(GetById), new { id = created.TransactionId }, _mapper.Map<PointTransactionDto>(created));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var transaction = await _pointTransactionRepository.GetByIdAsync(id);

        if (transaction == null || transaction.UserId != GetCurrentUserId())
            return NotFound($"Transaction with ID {id} was not found.");

        await _pointTransactionRepository.DeleteAsync(id);
        return NoContent();
    }
}
