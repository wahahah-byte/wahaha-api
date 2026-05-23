using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Handlers;
using wahaha.API.Handlers.UserInventory;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;

namespace wahaha.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserInventoryController : ControllerBase
{
    private readonly IRequestHandler<GetInventoryListRequest, PagedResult<UserInventoryDto>> _list;
    private readonly IRequestHandler<GetInventoryByIdRequest, UserInventoryDto> _byId;
    private readonly IRequestHandler<GetEquippedInventoryRequest, IEnumerable<UserInventoryDto>> _equipped;
    private readonly IRequestHandler<CreateInventoryEntryRequest, UserInventoryDto> _create;
    private readonly IRequestHandler<EquipInventoryRequest, Unit> _equip;
    private readonly IRequestHandler<UnequipInventoryRequest, Unit> _unequip;
    private readonly IRequestHandler<SetInventoryPositionRequest, Unit> _setPosition;
    private readonly IRequestHandler<DeleteInventoryEntryRequest, Unit> _delete;
    private readonly IRequestHandler<SellInventoryRequest, SellInventoryResponseDto> _sell;

    public UserInventoryController(
        IRequestHandler<GetInventoryListRequest, PagedResult<UserInventoryDto>> list,
        IRequestHandler<GetInventoryByIdRequest, UserInventoryDto> byId,
        IRequestHandler<GetEquippedInventoryRequest, IEnumerable<UserInventoryDto>> equipped,
        IRequestHandler<CreateInventoryEntryRequest, UserInventoryDto> create,
        IRequestHandler<EquipInventoryRequest, Unit> equip,
        IRequestHandler<UnequipInventoryRequest, Unit> unequip,
        IRequestHandler<SetInventoryPositionRequest, Unit> setPosition,
        IRequestHandler<DeleteInventoryEntryRequest, Unit> delete,
        IRequestHandler<SellInventoryRequest, SellInventoryResponseDto> sell)
    {
        _list = list;
        _byId = byId;
        _equipped = equipped;
        _create = create;
        _equip = equip;
        _unequip = unequip;
        _setPosition = setPosition;
        _delete = delete;
        _sell = sell;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("appUserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserInventoryDto>>> GetAll([FromQuery] UserInventoryFilterParams filters)
    {
        var result = await _list.HandleAsync(new GetInventoryListRequest(GetCurrentUserId(), filters));
        return result.Status == HandlerStatus.Ok
            ? Ok(result.Value)
            : result.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserInventoryDto>> GetById(int id)
    {
        var result = await _byId.HandleAsync(new GetInventoryByIdRequest(id, GetCurrentUserId()));
        return result.Status == HandlerStatus.Ok
            ? Ok(result.Value)
            : result.ToActionResult();
    }

    [HttpGet("equipped")]
    public async Task<ActionResult<IEnumerable<UserInventoryDto>>> GetEquipped()
    {
        var result = await _equipped.HandleAsync(new GetEquippedInventoryRequest(GetCurrentUserId()));
        return result.Status == HandlerStatus.Ok
            ? Ok(result.Value)
            : result.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult<UserInventoryDto>> Create(CreateUserInventoryDto dto)
    {
        var result = await _create.HandleAsync(new CreateInventoryEntryRequest(GetCurrentUserId(), dto));
        if (result.Status == HandlerStatus.Ok && result.Value != null)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value.InventoryId }, result.Value);
        }
        return result.ToActionResult();
    }

    [HttpPatch("{id}/equip")]
    public async Task<IActionResult> Equip(int id)
        => (await _equip.HandleAsync(new EquipInventoryRequest(id, GetCurrentUserId()))).ToActionResult();

    [HttpPatch("{id}/position")]
    public async Task<IActionResult> SetPosition(int id, [FromBody] UpdateInventoryPositionDto dto)
        => (await _setPosition.HandleAsync(new SetInventoryPositionRequest(id, GetCurrentUserId(), dto))).ToActionResult();

    [HttpPatch("{id}/unequip")]
    public async Task<IActionResult> Unequip(int id)
        => (await _unequip.HandleAsync(new UnequipInventoryRequest(id, GetCurrentUserId()))).ToActionResult();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => (await _delete.HandleAsync(new DeleteInventoryEntryRequest(id, GetCurrentUserId()))).ToActionResult();

    // Sell an inventory item back for a partial refund. Returns the refund amount + new balance.
    [HttpPatch("{id}/sell")]
    public async Task<ActionResult<SellInventoryResponseDto>> Sell(int id)
    {
        var result = await _sell.HandleAsync(new SellInventoryRequest(id, GetCurrentUserId()));
        return result.Status == HandlerStatus.Ok
            ? Ok(result.Value)
            : result.ToActionResult();
    }
}
