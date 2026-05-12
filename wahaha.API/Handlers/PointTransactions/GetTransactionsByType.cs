using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.PointTransactions;

public sealed record GetTransactionsByTypeRequest(Guid UserId, TransactionType Type);

public sealed class GetTransactionsByTypeHandler : IRequestHandler<GetTransactionsByTypeRequest, IEnumerable<PointTransactionDto>>
{
    private readonly IPointTransactionRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTransactionsByTypeHandler> _logger;

    public GetTransactionsByTypeHandler(IPointTransactionRepository repo, IMapper mapper, ILogger<GetTransactionsByTypeHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<IEnumerable<PointTransactionDto>>> HandleAsync(GetTransactionsByTypeRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching {Type} transactions for user {UserId}", request.Type, request.UserId);
        var ts = await _repo.GetByUserAndTypeAsync(request.UserId, request.Type);
        return HandlerResult<IEnumerable<PointTransactionDto>>.Ok(_mapper.Map<IEnumerable<PointTransactionDto>>(ts));
    }
}
