using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.AvatarItems;

public sealed record GetAvatarItemListRequest(AvatarItemFilterParams Filters);

public sealed class GetAvatarItemListHandler : IRequestHandler<GetAvatarItemListRequest, PagedResult<AvatarItemDto>>
{
    private readonly IAvatarItemRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAvatarItemListHandler> _logger;

    public GetAvatarItemListHandler(IAvatarItemRepository repo, IMapper mapper, ILogger<GetAvatarItemListHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<PagedResult<AvatarItemDto>>> HandleAsync(GetAvatarItemListRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching avatar items with filters");
        var result = await _repo.GetFilteredAsync(request.Filters);
        return HandlerResult<PagedResult<AvatarItemDto>>.Ok(new PagedResult<AvatarItemDto>
        {
            Data = _mapper.Map<IEnumerable<AvatarItemDto>>(result.Data),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
        });
    }
}
