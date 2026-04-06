using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Auth;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvatarItemsController : ControllerBase
{
    private readonly IAvatarItemRepository _avatarItemRepository;
    private readonly IBlobService _blobService;
    private readonly IMapper _mapper;
    private readonly ILogger<AvatarItemsController> _logger;
    private const string ContainerName = "avatar-items";

    public AvatarItemsController(
        IAvatarItemRepository avatarItemRepository,
        IBlobService blobService,
        IMapper mapper,
        ILogger<AvatarItemsController> logger)
    {
        _avatarItemRepository = avatarItemRepository;
        _blobService = blobService;
        _mapper = mapper;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedResult<AvatarItemDto>>> GetAll([FromQuery] AvatarItemFilterParams filters)
    {
        _logger.LogDebug("Fetching avatar items with filters");
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
        _logger.LogDebug("Fetching avatar item {ItemId}", id);
        var item = await _avatarItemRepository.GetByIdAsync(id);

        if (item == null)
        {
            _logger.LogWarning("Avatar item {ItemId} not found", id);
            return NotFound($"Avatar item with ID {id} was not found.");
        }

        return Ok(_mapper.Map<AvatarItemDto>(item));
    }

    [AllowAnonymous]
    [HttpGet("rarity/{rarity}")]
    public async Task<ActionResult<IEnumerable<AvatarItemDto>>> GetByRarity(Rarity rarity)
    {
        _logger.LogDebug("Fetching avatar items by rarity {Rarity}", rarity);
        var items = await _avatarItemRepository.GetByRarityAsync(rarity);
        return Ok(_mapper.Map<IEnumerable<AvatarItemDto>>(items));
    }

    [AllowAnonymous]
    [HttpGet("slot/{slot}")]
    public async Task<ActionResult<IEnumerable<AvatarItemDto>>> GetBySlot(ItemSlot slot)
    {
        _logger.LogDebug("Fetching avatar items by slot {Slot}", slot);
        var items = await _avatarItemRepository.GetBySlotAsync(slot);
        return Ok(_mapper.Map<IEnumerable<AvatarItemDto>>(items));
    }

    [Authorize(Roles = $"{WahahaUserRoles.Admin},{WahahaUserRoles.Moderator}")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AvatarItemDto>> Create([FromForm] CreateAvatarItemDto dto)
    {
        _logger.LogInformation("Creating avatar item {Name}", dto.Name);
        var item = _mapper.Map<AvatarItem>(dto);

        if (dto.Image != null)
        {
            if (!IsValidImage(dto.Image))
            {
                _logger.LogWarning("Invalid image upload for avatar item {Name}", dto.Name);
                return BadRequest("Invalid image. Only JPG, PNG and WebP files under 5MB are allowed.");
            }

            item.PreviewAssetUrl = await _blobService.UploadAsync(dto.Image, ContainerName);
            _logger.LogInformation("Image uploaded for avatar item {Name}: {Url}", dto.Name, item.PreviewAssetUrl);
        }

        var created = await _avatarItemRepository.CreateAsync(item);
        _logger.LogInformation("Avatar item {ItemId} ({Name}) created successfully", created.ItemId, created.Name);

        return CreatedAtAction(nameof(GetById), new { id = created.ItemId }, _mapper.Map<AvatarItemDto>(created));
    }

    [Authorize(Roles = $"{WahahaUserRoles.Admin},{WahahaUserRoles.Moderator}")]
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateAvatarItemDto dto)
    {
        if (id != dto.ItemId)
            return BadRequest("Item ID in the URL does not match the request body.");

        _logger.LogInformation("Updating avatar item {ItemId}", id);
        var item = await _avatarItemRepository.GetByIdAsync(id);

        if (item == null)
        {
            _logger.LogWarning("Avatar item {ItemId} not found for update", id);
            return NotFound($"Avatar item with ID {id} was not found.");
        }

        if (dto.Image != null)
        {
            if (!IsValidImage(dto.Image))
            {
                _logger.LogWarning("Invalid image upload for avatar item {ItemId}", id);
                return BadRequest("Invalid image. Only JPG, PNG and WebP files under 5MB are allowed.");
            }

            if (!string.IsNullOrEmpty(item.PreviewAssetUrl))
                await _blobService.DeleteAsync(item.PreviewAssetUrl, ContainerName);

            item.PreviewAssetUrl = await _blobService.UploadAsync(dto.Image, ContainerName);
            _logger.LogInformation("Image updated for avatar item {ItemId}: {Url}", id, item.PreviewAssetUrl);
        }

        _mapper.Map(dto, item);
        await _avatarItemRepository.UpdateAsync(item);

        _logger.LogInformation("Avatar item {ItemId} updated successfully", id);
        return NoContent();
    }

    [Authorize(Roles = $"{WahahaUserRoles.Admin},{WahahaUserRoles.Moderator}")]
    [HttpPatch("{id}/toggleavailability")]
    public async Task<IActionResult> ToggleAvailability(int id)
    {
        _logger.LogInformation("Toggling availability for avatar item {ItemId}", id);
        var success = await _avatarItemRepository.ToggleAvailabilityAsync(id);

        if (!success)
        {
            _logger.LogWarning("Avatar item {ItemId} not found for toggle", id);
            return NotFound($"Avatar item with ID {id} was not found.");
        }

        return NoContent();
    }

    [Authorize(Roles = WahahaUserRoles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting avatar item {ItemId}", id);
        var item = await _avatarItemRepository.GetByIdAsync(id);

        if (item == null)
        {
            _logger.LogWarning("Avatar item {ItemId} not found for deletion", id);
            return NotFound($"Avatar item with ID {id} was not found.");
        }

        if (!string.IsNullOrEmpty(item.PreviewAssetUrl))
            await _blobService.DeleteAsync(item.PreviewAssetUrl, ContainerName);

        await _avatarItemRepository.DeleteAsync(id);
        _logger.LogInformation("Avatar item {ItemId} deleted successfully", id);
        return NoContent();
    }

    private static bool IsValidImage(IFormFile file)
    {
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        var maxSize = 5 * 1024 * 1024;

        return allowedTypes.Contains(file.ContentType.ToLower())
            && file.Length <= maxSize;
    }
}
