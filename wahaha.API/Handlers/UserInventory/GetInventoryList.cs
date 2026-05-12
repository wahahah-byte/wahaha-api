using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.UserInventory;

public sealed record GetInventoryListRequest(Guid UserId, UserInventoryFilterParams Filters);

public sealed class GetInventoryListHandler
    : IRequestHandler<GetInventoryListRequest, PagedResult<UserInventoryDto>>
{
    private readonly IUserInventoryRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetInventoryListHandler> _logger;

    public GetInventoryListHandler(
        IUserInventoryRepository repo,
        IMapper mapper,
        ILogger<GetInventoryListHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<PagedResult<UserInventoryDto>>> HandleAsync(
        GetInventoryListRequest request,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching inventory for user {UserId}", request.UserId);
        request.Filters.UserId = request.UserId;
        var result = await _repo.GetFilteredAsync(request.Filters);
        return HandlerResult<PagedResult<UserInventoryDto>>.Ok(new PagedResult<UserInventoryDto>
        {
            Data = _mapper.Map<IEnumerable<UserInventoryDto>>(result.Data),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
        });
    }
}
