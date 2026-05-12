using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.PointTransactions;

public sealed record GetTransactionByIdRequest(int TransactionId, Guid UserId);

public sealed class GetTransactionByIdHandler : IRequestHandler<GetTransactionByIdRequest, PointTransactionDto>
{
    private readonly IPointTransactionRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTransactionByIdHandler> _logger;

    public GetTransactionByIdHandler(IPointTransactionRepository repo, IMapper mapper, ILogger<GetTransactionByIdHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<PointTransactionDto>> HandleAsync(GetTransactionByIdRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching transaction {TransactionId}", request.TransactionId);
        var t = await _repo.GetByIdAsync(request.TransactionId);
        if (t == null || t.UserId != request.UserId)
        {
            _logger.LogWarning("Transaction {TransactionId} not found or unauthorized", request.TransactionId);
            return HandlerResult<PointTransactionDto>.NotFound($"Transaction with ID {request.TransactionId} was not found.");
        }
        return HandlerResult<PointTransactionDto>.Ok(_mapper.Map<PointTransactionDto>(t));
    }
}
