using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Users;

public sealed record GetCurrentUserRequest(Guid UserId);

public sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserRequest, UserDto>
{
    private readonly IUserRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCurrentUserHandler> _logger;

    public GetCurrentUserHandler(IUserRepository repo, IMapper mapper, ILogger<GetCurrentUserHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<UserDto>> HandleAsync(GetCurrentUserRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching profile for user {UserId}", request.UserId);
        var user = await _repo.GetByIdWithTransactionsAsync(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", request.UserId);
            return HandlerResult<UserDto>.NotFound("User was not found.");
        }
        return HandlerResult<UserDto>.Ok(_mapper.Map<UserDto>(user));
    }
}
