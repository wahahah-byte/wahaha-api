using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;
using DomainUserInventory = wahaha.API.Models.Domain.UserInventory;

namespace wahaha.API.Handlers.UserInventory;

public sealed record CreateInventoryEntryRequest(Guid UserId, CreateUserInventoryDto Dto);

public sealed class CreateInventoryEntryHandler
    : IRequestHandler<CreateInventoryEntryRequest, UserInventoryDto>
{
    private readonly IUserInventoryRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateInventoryEntryHandler> _logger;

    public CreateInventoryEntryHandler(
        IUserInventoryRepository repo,
        IMapper mapper,
        ILogger<CreateInventoryEntryHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<UserInventoryDto>> HandleAsync(
        CreateInventoryEntryRequest request,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Adding item {ItemId} to inventory for user {UserId}",
            request.Dto.ItemId, request.UserId);

        var entry = _mapper.Map<DomainUserInventory>(request.Dto);
        entry.UserId = request.UserId;
        var created = await _repo.CreateAsync(entry);

        _logger.LogInformation("Item {ItemId} added to inventory {InventoryId} for user {UserId}",
            request.Dto.ItemId, created.InventoryId, request.UserId);

        return HandlerResult<UserInventoryDto>.Ok(_mapper.Map<UserInventoryDto>(created));
    }
}
