using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Streaks;

public sealed record GetStreakListRequest(Guid UserId, StreakFilterParams Filters);

public sealed class GetStreakListHandler : IRequestHandler<GetStreakListRequest, PagedResult<StreakDto>>
{
    private readonly IStreakRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetStreakListHandler> _logger;

    public GetStreakListHandler(IStreakRepository repo, IMapper mapper, ILogger<GetStreakListHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<PagedResult<StreakDto>>> HandleAsync(GetStreakListRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching streaks for user {UserId}", request.UserId);
        request.Filters.UserId = request.UserId;
        var result = await _repo.GetFilteredAsync(request.Filters);
        return HandlerResult<PagedResult<StreakDto>>.Ok(new PagedResult<StreakDto>
        {
            Data = _mapper.Map<IEnumerable<StreakDto>>(result.Data),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
        });
    }
}
