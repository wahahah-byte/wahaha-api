using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.MinigameSessions;

public sealed record CreateSessionRequest(Guid UserId, CreateMinigameSessionDto Dto);

public sealed class CreateSessionHandler : IRequestHandler<CreateSessionRequest, MinigameSessionDto>
{
    private readonly IMinigameSessionRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSessionHandler> _logger;

    public CreateSessionHandler(IMinigameSessionRepository repo, IMapper mapper, ILogger<CreateSessionHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<MinigameSessionDto>> HandleAsync(CreateSessionRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating session for game {GameId} by user {UserId}", request.Dto.GameId, request.UserId);
        var session = _mapper.Map<MinigameSession>(request.Dto);
        session.UserId = request.UserId;
        var created = await _repo.CreateAsync(session);
        _logger.LogInformation("Session {SessionId} created for game {GameId} by user {UserId}",
            created.SessionId, request.Dto.GameId, request.UserId);
        return HandlerResult<MinigameSessionDto>.Ok(_mapper.Map<MinigameSessionDto>(created));
    }
}
