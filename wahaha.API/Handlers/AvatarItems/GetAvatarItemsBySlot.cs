using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.AvatarItems;

public sealed record GetAvatarItemsBySlotRequest(ItemSlot Slot);

public sealed class GetAvatarItemsBySlotHandler : IRequestHandler<GetAvatarItemsBySlotRequest, IEnumerable<AvatarItemDto>>
{
    private readonly IAvatarItemRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAvatarItemsBySlotHandler> _logger;

    public GetAvatarItemsBySlotHandler(IAvatarItemRepository repo, IMapper mapper, ILogger<GetAvatarItemsBySlotHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<IEnumerable<AvatarItemDto>>> HandleAsync(GetAvatarItemsBySlotRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching avatar items by slot {Slot}", request.Slot);
        var items = await _repo.GetBySlotAsync(request.Slot);
        return HandlerResult<IEnumerable<AvatarItemDto>>.Ok(_mapper.Map<IEnumerable<AvatarItemDto>>(items));
    }
}
