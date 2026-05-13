using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Handlers;
using wahaha.API.Handlers.AvatarItems;
using wahaha.API.Models.Auth;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;

namespace wahaha.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvatarItemsController : ControllerBase
{
    private readonly IRequestHandler<GetAvatarItemListRequest, PagedResult<AvatarItemDto>> _list;
    private readonly IRequestHandler<GetAvatarItemByIdRequest, AvatarItemDto> _byId;
    private readonly IRequestHandler<GetAvatarItemsByRarityRequest, IEnumerable<AvatarItemDto>> _byRarity;
    private readonly IRequestHandler<GetAvatarItemsBySlotRequest, IEnumerable<AvatarItemDto>> _bySlot;
    private readonly IRequestHandler<CreateAvatarItemRequest, AvatarItemDto> _create;
    private readonly IRequestHandler<RegisterAvatarItemByUrlRequest, AvatarItemDto> _registerByUrl;
    private readonly IRequestHandler<UpdateAvatarItemRequest, Unit> _update;
    private readonly IRequestHandler<ToggleAvatarItemAvailabilityRequest, Unit> _toggle;
    private readonly IRequestHandler<RecomputeAvatarItemBoundsRequest, AvatarItemDto> _recomputeBounds;
    private readonly IRequestHandler<GrantAvatarItemRequest, UserInventoryDto> _grant;
    private readonly IRequestHandler<DeleteAvatarItemRequest, Unit> _delete;

    public AvatarItemsController(
        IRequestHandler<GetAvatarItemListRequest, PagedResult<AvatarItemDto>> list,
        IRequestHandler<GetAvatarItemByIdRequest, AvatarItemDto> byId,
        IRequestHandler<GetAvatarItemsByRarityRequest, IEnumerable<AvatarItemDto>> byRarity,
        IRequestHandler<GetAvatarItemsBySlotRequest, IEnumerable<AvatarItemDto>> bySlot,
        IRequestHandler<CreateAvatarItemRequest, AvatarItemDto> create,
        IRequestHandler<RegisterAvatarItemByUrlRequest, AvatarItemDto> registerByUrl,
        IRequestHandler<UpdateAvatarItemRequest, Unit> update,
        IRequestHandler<ToggleAvatarItemAvailabilityRequest, Unit> toggle,
        IRequestHandler<RecomputeAvatarItemBoundsRequest, AvatarItemDto> recomputeBounds,
        IRequestHandler<GrantAvatarItemRequest, UserInventoryDto> grant,
        IRequestHandler<DeleteAvatarItemRequest, Unit> delete)
    {
        _list = list;
        _byId = byId;
        _byRarity = byRarity;
        _bySlot = bySlot;
        _create = create;
        _registerByUrl = registerByUrl;
        _update = update;
        _toggle = toggle;
        _recomputeBounds = recomputeBounds;
        _grant = grant;
        _delete = delete;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedResult<AvatarItemDto>>> GetAll([FromQuery] AvatarItemFilterParams filters)
        => (await _list.HandleAsync(new GetAvatarItemListRequest(filters))).ToActionResult();

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<AvatarItemDto>> GetById(int id)
        => (await _byId.HandleAsync(new GetAvatarItemByIdRequest(id))).ToActionResult();

    [AllowAnonymous]
    [HttpGet("rarity/{rarity}")]
    public async Task<ActionResult<IEnumerable<AvatarItemDto>>> GetByRarity(Rarity rarity)
        => (await _byRarity.HandleAsync(new GetAvatarItemsByRarityRequest(rarity))).ToActionResult();

    [AllowAnonymous]
    [HttpGet("slot/{slot}")]
    public async Task<ActionResult<IEnumerable<AvatarItemDto>>> GetBySlot(ItemSlot slot)
        => (await _bySlot.HandleAsync(new GetAvatarItemsBySlotRequest(slot))).ToActionResult();

    [Authorize(Roles = $"{WahahaUserRoles.Admin},{WahahaUserRoles.Moderator}")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AvatarItemDto>> Create([FromForm] CreateAvatarItemDto dto)
    {
        var result = await _create.HandleAsync(new CreateAvatarItemRequest(dto));
        if (result.Status == HandlerStatus.Ok && result.Value != null)
            return CreatedAtAction(nameof(GetById), new { id = result.Value.ItemId }, result.Value);
        return result.ToActionResult();
    }

    [Authorize(Roles = $"{WahahaUserRoles.Admin},{WahahaUserRoles.Moderator}")]
    [HttpPost("register-by-url")]
    public async Task<ActionResult<AvatarItemDto>> RegisterByUrl([FromBody] RegisterAvatarItemByUrlDto dto)
    {
        var result = await _registerByUrl.HandleAsync(new RegisterAvatarItemByUrlRequest(GetCurrentUserId(), dto));
        if (result.Status == HandlerStatus.Ok && result.Value != null)
            return CreatedAtAction(nameof(GetById), new { id = result.Value.ItemId }, result.Value);
        return result.ToActionResult();
    }

    [Authorize(Roles = $"{WahahaUserRoles.Admin},{WahahaUserRoles.Moderator}")]
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateAvatarItemDto dto)
        => (await _update.HandleAsync(new UpdateAvatarItemRequest(id, dto))).ToActionResult();

    [Authorize(Roles = $"{WahahaUserRoles.Admin},{WahahaUserRoles.Moderator}")]
    [HttpPatch("{id}/toggleavailability")]
    public async Task<IActionResult> ToggleAvailability(int id)
        => (await _toggle.HandleAsync(new ToggleAvatarItemAvailabilityRequest(id))).ToActionResult();

    // Re-scans the item's PreviewAssetUrl and stores fresh content bounds.
    // Returns the updated DTO so the admin UI can splice it in without a
    // full re-fetch. See RecomputeAvatarItemBoundsHandler for the supported
    // URL shapes — only absolute http(s) URLs work; relative seed paths
    // need to be edited and re-uploaded through the form instead.
    [Authorize(Roles = $"{WahahaUserRoles.Admin},{WahahaUserRoles.Moderator}")]
    [HttpPost("{id}/recompute-bounds")]
    public async Task<ActionResult<AvatarItemDto>> RecomputeBounds(int id)
        => (await _recomputeBounds.HandleAsync(new RecomputeAvatarItemBoundsRequest(id))).ToActionResult();

    // Admin convenience for handing an item to a user without going
    // through the regular acquire flow (which charges points). Body's
    // TargetEmail is optional — empty / missing grants to the calling
    // admin themselves. AutoEquip flips the new inventory row to
    // equipped (unequipping any other item in the same slot).
    [Authorize(Roles = $"{WahahaUserRoles.Admin},{WahahaUserRoles.Moderator}")]
    [HttpPost("{id}/grant")]
    public async Task<ActionResult<UserInventoryDto>> Grant(int id, [FromBody] GrantAvatarItemDto dto)
        => (await _grant.HandleAsync(new GrantAvatarItemRequest(
            GetCurrentUserId(), id, dto?.TargetEmail, dto?.AutoEquip ?? false))).ToActionResult();

    [Authorize(Roles = WahahaUserRoles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => (await _delete.HandleAsync(new DeleteAvatarItemRequest(id))).ToActionResult();
}
