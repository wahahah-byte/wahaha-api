using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.UserInventory;

public sealed record GetInventoryByIdRequest(int InventoryId, Guid UserId);

public sealed class GetInventoryByIdHandler
    : IRequestHandler<GetInventoryByIdRequest, UserInventoryDto>
{
    private readonly IUserInventoryRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetInventoryByIdHandler> _logger;

    public GetInventoryByIdHandler(
        IUserInventoryRepository repo,
        IMapper mapper,
        ILogger<GetInventoryByIdHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<UserInventoryDto>> HandleAsync(
        GetInventoryByIdRequest request,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching inventory entry {InventoryId}", request.InventoryId);
        var entry = await _repo.GetByIdAsync(request.InventoryId);
        if (entry == null || entry.UserId != request.UserId)
        {
            _logger.LogWarning("Inventory entry {InventoryId} not found or unauthorized", request.InventoryId);
            return HandlerResult<UserInventoryDto>.NotFound($"Inventory entry with ID {request.InventoryId} was not found.");
        }
        return HandlerResult<UserInventoryDto>.Ok(_mapper.Map<UserInventoryDto>(entry));
    }
}
