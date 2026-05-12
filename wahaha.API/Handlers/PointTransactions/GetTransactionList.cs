using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.PointTransactions;

public sealed record GetTransactionListRequest(Guid UserId, PointTransactionFilterParams Filters);

public sealed class GetTransactionListHandler : IRequestHandler<GetTransactionListRequest, PagedResult<PointTransactionDto>>
{
    private readonly IPointTransactionRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTransactionListHandler> _logger;

    public GetTransactionListHandler(IPointTransactionRepository repo, IMapper mapper, ILogger<GetTransactionListHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<PagedResult<PointTransactionDto>>> HandleAsync(GetTransactionListRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching point transactions for user {UserId}", request.UserId);
        request.Filters.UserId = request.UserId;
        var result = await _repo.GetFilteredAsync(request.Filters);
        return HandlerResult<PagedResult<PointTransactionDto>>.Ok(new PagedResult<PointTransactionDto>
        {
            Data = _mapper.Map<IEnumerable<PointTransactionDto>>(result.Data),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
        });
    }
}
