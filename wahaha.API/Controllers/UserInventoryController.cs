using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTOs;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Controllers;

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserInventoryDto>>> GetAll()
    {
        var inventory = await _inventoryRepository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<UserInventoryDto>>(inventory));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserInventoryDto>> GetById(int id)
    {
        var entry = await _inventoryRepository.GetByIdAsync(id);

        if (entry == null)
            return NotFound($"Inventory entry with ID {id} was not found.");

        return Ok(_mapper.Map<UserInventoryDto>(entry));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<UserInventoryDto>>> GetByUser(Guid userId)
    {
        var inventory = await _inventoryRepository.GetByUserAsync(userId);
        return Ok(_mapper.Map<IEnumerable<UserInventoryDto>>(inventory));
    }

    [HttpGet("user/{userId}/equipped")]
    public async Task<ActionResult<IEnumerable<UserInventoryDto>>> GetEquipped(Guid userId)
    {
        var equipped = await _inventoryRepository.GetEquippedByUserAsync(userId);
        return Ok(_mapper.Map<IEnumerable<UserInventoryDto>>(equipped));
    }

    [HttpPost]
    public async Task<ActionResult<UserInventoryDto>> Create(CreateUserInventoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entry = _mapper.Map<UserInventory>(dto);
        var created = await _inventoryRepository.CreateAsync(entry);

        return CreatedAtAction(nameof(GetById), new { id = created.InventoryId }, _mapper.Map<UserInventoryDto>(created));
    }

    [HttpPatch("{id}/equip")]
    public async Task<IActionResult> Equip(int id)
    {
        var success = await _inventoryRepository.EquipAsync(id);

        if (!success)
            return NotFound($"Inventory entry with ID {id} was not found.");

        return NoContent();
    }

    [HttpPatch("{id}/unequip")]
    public async Task<IActionResult> Unequip(int id)
    {
        var success = await _inventoryRepository.UnequipAsync(id);

        if (!success)
            return NotFound($"Inventory entry with ID {id} was not found.");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _inventoryRepository.DeleteAsync(id);

        if (!success)
            return NotFound($"Inventory entry with ID {id} was not found.");

        return NoContent();
    }
}
