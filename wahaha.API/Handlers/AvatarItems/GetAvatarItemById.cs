using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.AvatarItems;

public sealed record GetAvatarItemByIdRequest(int ItemId);

public sealed class GetAvatarItemByIdHandler : IRequestHandler<GetAvatarItemByIdRequest, AvatarItemDto>
{
    private readonly IAvatarItemRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAvatarItemByIdHandler> _logger;

    public GetAvatarItemByIdHandler(IAvatarItemRepository repo, IMapper mapper, ILogger<GetAvatarItemByIdHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<AvatarItemDto>> HandleAsync(GetAvatarItemByIdRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching avatar item {ItemId}", request.ItemId);
        var item = await _repo.GetByIdAsync(request.ItemId);
        if (item == null)
        {
            _logger.LogWarning("Avatar item {ItemId} not found", request.ItemId);
            return HandlerResult<AvatarItemDto>.NotFound($"Avatar item with ID {request.ItemId} was not found.");
        }
        return HandlerResult<AvatarItemDto>.Ok(_mapper.Map<AvatarItemDto>(item));
    }
}
