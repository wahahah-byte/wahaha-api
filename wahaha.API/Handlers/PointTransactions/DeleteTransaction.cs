using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.PointTransactions;

public sealed record DeleteTransactionRequest(int TransactionId, Guid UserId);

public sealed class DeleteTransactionHandler : IRequestHandler<DeleteTransactionRequest, Unit>
{
    private readonly IPointTransactionRepository _repo;
    private readonly ILogger<DeleteTransactionHandler> _logger;

    public DeleteTransactionHandler(IPointTransactionRepository repo, ILogger<DeleteTransactionHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(DeleteTransactionRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting transaction {TransactionId}", request.TransactionId);
        var t = await _repo.GetByIdAsync(request.TransactionId);
        if (t == null || t.UserId != request.UserId)
        {
            _logger.LogWarning("Transaction {TransactionId} not found or unauthorized for deletion", request.TransactionId);
            return HandlerResult<Unit>.NotFound($"Transaction with ID {request.TransactionId} was not found.");
        }
        await _repo.DeleteAsync(request.TransactionId);
        _logger.LogInformation("Transaction {TransactionId} deleted successfully", request.TransactionId);
        return HandlerResult<Unit>.NoContent();
    }
}
