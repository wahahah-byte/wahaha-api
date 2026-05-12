using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.MinigameSessions;

public sealed record GetSessionByIdRequest(int SessionId, Guid UserId);

public sealed class GetSessionByIdHandler : IRequestHandler<GetSessionByIdRequest, MinigameSessionDto>
{
    private readonly IMinigameSessionRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSessionByIdHandler> _logger;

    public GetSessionByIdHandler(IMinigameSessionRepository repo, IMapper mapper, ILogger<GetSessionByIdHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<MinigameSessionDto>> HandleAsync(GetSessionByIdRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching session {SessionId}", request.SessionId);
        var session = await _repo.GetByIdAsync(request.SessionId);
        if (session == null || session.UserId != request.UserId)
        {
            _logger.LogWarning("Session {SessionId} not found or unauthorized", request.SessionId);
            return HandlerResult<MinigameSessionDto>.NotFound($"Session with ID {request.SessionId} was not found.");
        }
        return HandlerResult<MinigameSessionDto>.Ok(_mapper.Map<MinigameSessionDto>(session));
    }
}
