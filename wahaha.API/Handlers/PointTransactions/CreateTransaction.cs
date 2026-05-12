using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.PointTransactions;

public sealed record CreateTransactionRequest(Guid UserId, CreatePointTransactionDto Dto);

public sealed class CreateTransactionHandler : IRequestHandler<CreateTransactionRequest, PointTransactionDto>
{
    private readonly IPointTransactionRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateTransactionHandler> _logger;

    public CreateTransactionHandler(IPointTransactionRepository repo, IMapper mapper, ILogger<CreateTransactionHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<PointTransactionDto>> HandleAsync(CreateTransactionRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating {Type} transaction of {Amount} points for user {UserId}",
            request.Dto.Type, request.Dto.Amount, request.UserId);
        var t = _mapper.Map<PointTransaction>(request.Dto);
        t.UserId = request.UserId;
        var created = await _repo.CreateAsync(t);
        _logger.LogInformation("Transaction {TransactionId} created for user {UserId}", created.TransactionId, request.UserId);
        return HandlerResult<PointTransactionDto>.Ok(_mapper.Map<PointTransactionDto>(created));
    }
}
