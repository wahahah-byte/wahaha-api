using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.UserInventory;

public sealed record GetEquippedInventoryRequest(Guid UserId);

public sealed class GetEquippedInventoryHandler
    : IRequestHandler<GetEquippedInventoryRequest, IEnumerable<UserInventoryDto>>
{
    private readonly IUserInventoryRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetEquippedInventoryHandler> _logger;

    public GetEquippedInventoryHandler(
        IUserInventoryRepository repo,
        IMapper mapper,
        ILogger<GetEquippedInventoryHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<IEnumerable<UserInventoryDto>>> HandleAsync(
        GetEquippedInventoryRequest request,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching equipped items for user {UserId}", request.UserId);
        var equipped = await _repo.GetEquippedByUserAsync(request.UserId);
        return HandlerResult<IEnumerable<UserInventoryDto>>.Ok(_mapper.Map<IEnumerable<UserInventoryDto>>(equipped));
    }
}
