using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Users;

public sealed record UpdateUserRequest(Guid UserId, UpdateUserDto Dto);

public sealed class UpdateUserHandler : IRequestHandler<UpdateUserRequest, Unit>
{
    private readonly IUserRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateUserHandler> _logger;

    public UpdateUserHandler(IUserRepository repo, IMapper mapper, ILogger<UpdateUserHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(UpdateUserRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating profile for user {UserId}", request.UserId);
        var user = await _repo.GetByIdAsync(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for update", request.UserId);
            return HandlerResult<Unit>.NotFound("User was not found.");
        }

        request.Dto.UserId = request.UserId;
        _mapper.Map(request.Dto, user);
        await _repo.UpdateAsync(user);
        _logger.LogInformation("Profile updated for user {UserId}", request.UserId);
        return HandlerResult<Unit>.NoContent();
    }
}
