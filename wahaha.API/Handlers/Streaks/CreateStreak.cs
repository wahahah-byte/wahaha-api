using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Streaks;

public sealed record CreateStreakRequest(Guid UserId, CreateStreakDto Dto);

public sealed class CreateStreakHandler : IRequestHandler<CreateStreakRequest, StreakDto>
{
    private readonly IStreakRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateStreakHandler> _logger;

    public CreateStreakHandler(IStreakRepository repo, IMapper mapper, ILogger<CreateStreakHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<StreakDto>> HandleAsync(CreateStreakRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating streak {StreakType} for user {UserId}", request.Dto.StreakType, request.UserId);
        var streak = _mapper.Map<Streak>(request.Dto);
        streak.UserId = request.UserId;
        var created = await _repo.CreateAsync(streak);
        _logger.LogInformation("Streak {StreakId} created for user {UserId}", created.StreakId, request.UserId);
        return HandlerResult<StreakDto>.Ok(_mapper.Map<StreakDto>(created));
    }
}
