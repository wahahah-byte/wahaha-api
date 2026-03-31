using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTOs;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvatarItemsController : ControllerBase
{
    private readonly IAvatarItemRepository _avatarItemRepository;
    private readonly IMapper _mapper;

    public AvatarItemsController(IAvatarItemRepository avatarItemRepository, IMapper mapper)
    {
        _avatarItemRepository = avatarItemRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AvatarItemDto>>> GetAll()
    {
        var items = await _avatarItemRepository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<AvatarItemDto>>(items));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AvatarItemDto>> GetById(int id)
    {
        var item = await _avatarItemRepository.GetByIdAsync(id);

        if (item == null)
            return NotFound($"Avatar item with ID {id} was not found.");

        return Ok(_mapper.Map<AvatarItemDto>(item));
    }

    [HttpGet("rarity/{rarity}")]
    public async Task<ActionResult<IEnumerable<AvatarItemDto>>> GetByRarity(Rarity rarity)
    {
        var items = await _avatarItemRepository.GetByRarityAsync(rarity);
        return Ok(_mapper.Map<IEnumerable<AvatarItemDto>>(items));
    }

    [HttpGet("slot/{slot}")]
    public async Task<ActionResult<IEnumerable<AvatarItemDto>>> GetBySlot(ItemSlot slot)
    {
        var items = await _avatarItemRepository.GetBySlotAsync(slot);
        return Ok(_mapper.Map<IEnumerable<AvatarItemDto>>(items));
    }

    [HttpPost]
    public async Task<ActionResult<AvatarItemDto>> Create(CreateAvatarItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var item = _mapper.Map<AvatarItem>(dto);
        var created = await _avatarItemRepository.CreateAsync(item);

        return CreatedAtAction(nameof(GetById), new { id = created.ItemId }, _mapper.Map<AvatarItemDto>(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateAvatarItemDto dto)
    {
        if (id != dto.ItemId)
            return BadRequest("Item ID in the URL does not match the request body.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var item = await _avatarItemRepository.GetByIdAsync(id);

        if (item == null)
            return NotFound($"Avatar item with ID {id} was not found.");

        _mapper.Map(dto, item);
        await _avatarItemRepository.UpdateAsync(item);

        return NoContent();
    }

    [HttpPatch("{id}/toggleavailability")]
    public async Task<IActionResult> ToggleAvailability(int id)
    {
        var success = await _avatarItemRepository.ToggleAvailabilityAsync(id);

        if (!success)
            return NotFound($"Avatar item with ID {id} was not found.");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _avatarItemRepository.DeleteAsync(id);

        if (!success)
            return NotFound($"Avatar item with ID {id} was not found.");

        return NoContent();
    }
}
