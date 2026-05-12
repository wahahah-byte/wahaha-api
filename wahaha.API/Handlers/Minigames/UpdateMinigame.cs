using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Minigames;

public sealed record UpdateMinigameRequest(int GameId, UpdateMinigameDto Dto);

public sealed class UpdateMinigameHandler : IRequestHandler<UpdateMinigameRequest, Unit>
{
    private readonly IMinigameRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateMinigameHandler> _logger;

    public UpdateMinigameHandler(IMinigameRepository repo, IMapper mapper, ILogger<UpdateMinigameHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(UpdateMinigameRequest request, CancellationToken ct = default)
    {
        if (request.GameId != request.Dto.GameId)
            return HandlerResult<Unit>.BadRequest("Game ID in the URL does not match the request body.");

        _logger.LogInformation("Updating minigame {GameId}", request.GameId);
        var game = await _repo.GetByIdAsync(request.GameId);
        if (game == null)
        {
            _logger.LogWarning("Minigame {GameId} not found for update", request.GameId);
            return HandlerResult<Unit>.NotFound($"Minigame with ID {request.GameId} was not found.");
        }

        _mapper.Map(request.Dto, game);
        await _repo.UpdateAsync(game);
        _logger.LogInformation("Minigame {GameId} updated successfully", request.GameId);
        return HandlerResult<Unit>.NoContent();
    }
}
