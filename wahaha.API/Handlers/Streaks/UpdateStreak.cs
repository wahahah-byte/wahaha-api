using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Streaks;

public sealed record UpdateStreakRequest(int StreakId, Guid UserId, UpdateStreakDto Dto);

public sealed class UpdateStreakHandler : IRequestHandler<UpdateStreakRequest, Unit>
{
    private readonly IStreakRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateStreakHandler> _logger;

    public UpdateStreakHandler(IStreakRepository repo, IMapper mapper, ILogger<UpdateStreakHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(UpdateStreakRequest request, CancellationToken ct = default)
    {
        if (request.StreakId != request.Dto.StreakId)
            return HandlerResult<Unit>.BadRequest("Streak ID in the URL does not match the request body.");

        _logger.LogInformation("Updating streak {StreakId}", request.StreakId);
        var streak = await _repo.GetByIdAsync(request.StreakId);
        if (streak == null || streak.UserId != request.UserId)
        {
            _logger.LogWarning("Streak {StreakId} not found or unauthorized for update", request.StreakId);
            return HandlerResult<Unit>.NotFound($"Streak with ID {request.StreakId} was not found.");
        }

        _mapper.Map(request.Dto, streak);
        await _repo.UpdateAsync(streak);
        _logger.LogInformation("Streak {StreakId} updated successfully", request.StreakId);
        return HandlerResult<Unit>.NoContent();
    }
}
