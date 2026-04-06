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
public class UserInventoryController : ControllerBase
{
    private readonly IUserInventoryRepository _inventoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserInventoryController> _logger;

    public UserInventoryController(
        IUserInventoryRepository inventoryRepository,
        IMapper mapper,
        ILogger<UserInventoryController> logger)
    {
        _inventoryRepository = inventoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserInventoryDto>>> GetAll([FromQuery] UserInventoryFilterParams filters)
    {
        var userId = GetCurrentUserId();
        filters.UserId = userId;
        _logger.LogDebug("Fetching inventory for user {UserId}", userId);

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
        _logger.LogDebug("Fetching inventory entry {InventoryId}", id);
        var entry = await _inventoryRepository.GetByIdAsync(id);

        if (entry == null || entry.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Inventory entry {InventoryId} not found or unauthorized", id);
            return NotFound($"Inventory entry with ID {id} was not found.");
        }

        return Ok(_mapper.Map<UserInventoryDto>(entry));
    }

    [HttpGet("equipped")]
    public async Task<ActionResult<IEnumerable<UserInventoryDto>>> GetEquipped()
    {
        var userId = GetCurrentUserId();
        _logger.LogDebug("Fetching equipped items for user {UserId}", userId);

        var equipped = await _inventoryRepository.GetEquippedByUserAsync(userId);
        return Ok(_mapper.Map<IEnumerable<UserInventoryDto>>(equipped));
    }

    [HttpPost]
    public async Task<ActionResult<UserInventoryDto>> Create(CreateUserInventoryDto dto)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Adding item {ItemId} to inventory for user {UserId}", dto.ItemId, userId);

        var entry = _mapper.Map<UserInventory>(dto);
        entry.UserId = userId;
        var created = await _inventoryRepository.CreateAsync(entry);

        _logger.LogInformation("Item {ItemId} added to inventory {InventoryId} for user {UserId}",
            dto.ItemId, created.InventoryId, userId);

        return CreatedAtAction(nameof(GetById), new { id = created.InventoryId },
            _mapper.Map<UserInventoryDto>(created));
    }

    [HttpPatch("{id}/equip")]
    public async Task<IActionResult> Equip(int id)
    {
        _logger.LogInformation("Equipping inventory item {InventoryId}", id);
        var entry = await _inventoryRepository.GetByIdAsync(id);

        if (entry == null || entry.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Inventory entry {InventoryId} not found or unauthorized for equip", id);
            return NotFound($"Inventory entry with ID {id} was not found.");
        }

        await _inventoryRepository.EquipAsync(id);
        _logger.LogInformation("Inventory item {InventoryId} equipped successfully", id);
        return NoContent();
    }

    [HttpPatch("{id}/unequip")]
    public async Task<IActionResult> Unequip(int id)
    {
        _logger.LogInformation("Unequipping inventory item {InventoryId}", id);
        var entry = await _inventoryRepository.GetByIdAsync(id);

        if (entry == null || entry.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Inventory entry {InventoryId} not found or unauthorized for unequip", id);
            return NotFound($"Inventory entry with ID {id} was not found.");
        }

        await _inventoryRepository.UnequipAsync(id);
        _logger.LogInformation("Inventory item {InventoryId} unequipped successfully", id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting inventory entry {InventoryId}", id);
        var entry = await _inventoryRepository.GetByIdAsync(id);

        if (entry == null || entry.UserId != GetCurrentUserId())
        {
            _logger.LogWarning("Inventory entry {InventoryId} not found or unauthorized for deletion", id);
            return NotFound($"Inventory entry with ID {id} was not found.");
        }

        await _inventoryRepository.DeleteAsync(id);
        _logger.LogInformation("Inventory entry {InventoryId} deleted successfully", id);
        return NoContent();
    }
}
