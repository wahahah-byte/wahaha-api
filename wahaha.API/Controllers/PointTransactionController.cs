using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTOs;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Controllers;

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PointTransactionDto>>> GetAll()
    {
        var transactions = await _pointTransactionRepository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<PointTransactionDto>>(transactions));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PointTransactionDto>> GetById(int id)
    {
        var transaction = await _pointTransactionRepository.GetByIdAsync(id);

        if (transaction == null)
            return NotFound($"Transaction with ID {id} was not found.");

        return Ok(_mapper.Map<PointTransactionDto>(transaction));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<PointTransactionDto>>> GetByUser(Guid userId)
    {
        var transactions = await _pointTransactionRepository.GetByUserAsync(userId);
        return Ok(_mapper.Map<IEnumerable<PointTransactionDto>>(transactions));
    }

    [HttpGet("user/{userId}/type/{type}")]
    public async Task<ActionResult<IEnumerable<PointTransactionDto>>> GetByUserAndType(Guid userId, TransactionType type)
    {
        var transactions = await _pointTransactionRepository.GetByUserAndTypeAsync(userId, type);
        return Ok(_mapper.Map<IEnumerable<PointTransactionDto>>(transactions));
    }

    [HttpPost]
    public async Task<ActionResult<PointTransactionDto>> Create(CreatePointTransactionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var transaction = _mapper.Map<PointTransaction>(dto);
        var created = await _pointTransactionRepository.CreateAsync(transaction);

        return CreatedAtAction(nameof(GetById), new { id = created.TransactionId }, _mapper.Map<PointTransactionDto>(created));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _pointTransactionRepository.DeleteAsync(id);

        if (!success)
            return NotFound($"Transaction with ID {id} was not found.");

        return NoContent();
    }
}
