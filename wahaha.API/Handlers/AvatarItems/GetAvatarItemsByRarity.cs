using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.AvatarItems;

public sealed record GetAvatarItemsByRarityRequest(Rarity Rarity);

public sealed class GetAvatarItemsByRarityHandler : IRequestHandler<GetAvatarItemsByRarityRequest, IEnumerable<AvatarItemDto>>
{
    private readonly IAvatarItemRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAvatarItemsByRarityHandler> _logger;

    public GetAvatarItemsByRarityHandler(IAvatarItemRepository repo, IMapper mapper, ILogger<GetAvatarItemsByRarityHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<IEnumerable<AvatarItemDto>>> HandleAsync(GetAvatarItemsByRarityRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching avatar items by rarity {Rarity}", request.Rarity);
        var items = await _repo.GetByRarityAsync(request.Rarity);
        return HandlerResult<IEnumerable<AvatarItemDto>>.Ok(_mapper.Map<IEnumerable<AvatarItemDto>>(items));
    }
}
