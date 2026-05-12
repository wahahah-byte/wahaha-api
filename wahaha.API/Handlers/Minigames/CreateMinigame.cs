using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Minigames;

public sealed record CreateMinigameRequest(CreateMinigameDto Dto);

public sealed class CreateMinigameHandler : IRequestHandler<CreateMinigameRequest, MinigameDto>
{
    private readonly IMinigameRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateMinigameHandler> _logger;

    public CreateMinigameHandler(IMinigameRepository repo, IMapper mapper, ILogger<CreateMinigameHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<MinigameDto>> HandleAsync(CreateMinigameRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating minigame {Name}", request.Dto.Name);
        var game = _mapper.Map<Minigame>(request.Dto);
        var created = await _repo.CreateAsync(game);
        _logger.LogInformation("Minigame {GameId} ({Name}) created successfully", created.GameId, created.Name);
        return HandlerResult<MinigameDto>.Ok(_mapper.Map<MinigameDto>(created));
    }
}
