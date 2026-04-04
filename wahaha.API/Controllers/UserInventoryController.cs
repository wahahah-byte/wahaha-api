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
public class UserInventoryController : ControllerBase
{
    private readonly IUserInventoryRepository _inventoryRepository;
    private readonly IMapper _mapper;

    public UserInventoryController(IUserInventoryRepository inventoryRepository, IMapper mapper)
    {
        _inventoryRepository = inventoryRepository;
        _mapper = mapper;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserInventoryDto>>> GetAll([FromQuery] UserInventoryFilterParams filters)
    {
        filters.UserId = GetCurrentUserId();
        var result = await _inventoryRepository.GetFilteredAsync(filters);

        return Ok(new PagedResult<UserInventoryDto>
        {
            Data = _mapper.Map<IEnumerable<UserInventoryDto>>(result.Data),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserInventoryDto>> GetById(int id)
    {
        var entry = await _inventoryRepository.GetByIdAsync(id);

        if (entry == null || entry.UserId != GetCurrentUserId())
            return NotFound($"Inventory entry with ID {id} was not found.");

        return Ok(_mapper.Map<UserInventoryDto>(entry));
    }

    [HttpGet("equipped")]
    public async Task<ActionResult<IEnumerable<UserInventoryDto>>> GetEquipped()
    {
        var equipped = await _inventoryRepository.GetEquippedByUserAsync(GetCurrentUserId());
        return Ok(_mapper.Map<IEnumerable<UserInventoryDto>>(equipped));
    }

    [HttpPost]
    public async Task<ActionResult<UserInventoryDto>> Create(CreateUserInventoryDto dto)
    {
        var entry = _mapper.Map<UserInventory>(dto);
        entry.UserId = GetCurrentUserId();
        var created = await _inventoryRepository.CreateAsync(entry);

        return CreatedAtAction(nameof(GetById), new { id = created.InventoryId }, _mapper.Map<UserInventoryDto>(created));
    }

    [HttpPatch("{id}/equip")]
    public async Task<IActionResult> Equip(int id)
    {
        var entry = await _inventoryRepository.GetByIdAsync(id);

        if (entry == null || entry.UserId != GetCurrentUserId())
            return NotFound($"Inventory entry with ID {id} was not found.");

        await _inventoryRepository.EquipAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/unequip")]
    public async Task<IActionResult> Unequip(int id)
    {
        var entry = await _inventoryRepository.GetByIdAsync(id);

        if (entry == null || entry.UserId != GetCurrentUserId())
            return NotFound($"Inventory entry with ID {id} was not found.");

        await _inventoryRepository.UnequipAsync(id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await _inventoryRepository.GetByIdAsync(id);

        if (entry == null || entry.UserId != GetCurrentUserId())
            return NotFound($"Inventory entry with ID {id} was not found.");

        await _inventoryRepository.DeleteAsync(id);
        return NoContent();
    }
}
