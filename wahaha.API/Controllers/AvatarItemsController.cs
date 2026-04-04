using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Auth;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTOs;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
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

    // Public — anyone can browse the shop
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedResult<AvatarItemDto>>> GetAll([FromQuery] AvatarItemFilterParams filters)
    {
        var result = await _avatarItemRepository.GetFilteredAsync(filters);

        return Ok(new PagedResult<AvatarItemDto>
        {
            Data = _mapper.Map<IEnumerable<AvatarItemDto>>(result.Data),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        });
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<AvatarItemDto>> GetById(int id)
    {
        var item = await _avatarItemRepository.GetByIdAsync(id);

        if (item == null)
            return NotFound($"Avatar item with ID {id} was not found.");

        return Ok(_mapper.Map<AvatarItemDto>(item));
    }

    [AllowAnonymous]
    [HttpGet("rarity/{rarity}")]
    public async Task<ActionResult<IEnumerable<AvatarItemDto>>> GetByRarity(Rarity rarity)
    {
        var items = await _avatarItemRepository.GetByRarityAsync(rarity);
        return Ok(_mapper.Map<IEnumerable<AvatarItemDto>>(items));
    }

    [AllowAnonymous]
    [HttpGet("slot/{slot}")]
    public async Task<ActionResult<IEnumerable<AvatarItemDto>>> GetBySlot(ItemSlot slot)
    {
        var items = await _avatarItemRepository.GetBySlotAsync(slot);
        return Ok(_mapper.Map<IEnumerable<AvatarItemDto>>(items));
    }

    // Admin and Moderator can create and update items
    [Authorize(Roles = $"{WahahaUserRoles.Admin},{WahahaUserRoles.Moderator}")]
    [HttpPost]
    public async Task<ActionResult<AvatarItemDto>> Create(CreateAvatarItemDto dto)
    {
        var item = _mapper.Map<AvatarItem>(dto);
        var created = await _avatarItemRepository.CreateAsync(item);

        return CreatedAtAction(nameof(GetById), new { id = created.ItemId }, _mapper.Map<AvatarItemDto>(created));
    }

    [Authorize(Roles = $"{WahahaUserRoles.Admin},{WahahaUserRoles.Moderator}")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateAvatarItemDto dto)
    {
        if (id != dto.ItemId)
            return BadRequest("Item ID in the URL does not match the request body.");

        var item = await _avatarItemRepository.GetByIdAsync(id);

        if (item == null)
            return NotFound($"Avatar item with ID {id} was not found.");

        _mapper.Map(dto, item);
        await _avatarItemRepository.UpdateAsync(item);

        return NoContent();
    }

    [Authorize(Roles = $"{WahahaUserRoles.Admin},{WahahaUserRoles.Moderator}")]
    [HttpPatch("{id}/toggleavailability")]
    public async Task<IActionResult> ToggleAvailability(int id)
    {
        var success = await _avatarItemRepository.ToggleAvailabilityAsync(id);

        if (!success)
            return NotFound($"Avatar item with ID {id} was not found.");

        return NoContent();
    }

    // Only Admin can delete items
    [Authorize(Roles = WahahaUserRoles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _avatarItemRepository.DeleteAsync(id);

        if (!success)
            return NotFound($"Avatar item with ID {id} was not found.");

        return NoContent();
    }
}
