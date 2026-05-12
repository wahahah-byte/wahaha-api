using Microsoft.AspNetCore.Identity;
using wahaha.API.Models.Auth;

namespace wahaha.API.Handlers.Admin;

public sealed record GetUserRolesRequest(string Email);

public sealed record UserRolesDto(string Email, IList<string> Roles);

public sealed class GetUserRolesHandler : IRequestHandler<GetUserRolesRequest, UserRolesDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GetUserRolesHandler> _logger;

    public GetUserRolesHandler(UserManager<ApplicationUser> userManager, ILogger<GetUserRolesHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<HandlerResult<UserRolesDto>> HandleAsync(GetUserRolesRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching roles for {Email}", request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("User {Email} not found when fetching roles", request.Email);
            return HandlerResult<UserRolesDto>.NotFound($"User with email {request.Email} was not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return HandlerResult<UserRolesDto>.Ok(new UserRolesDto(request.Email, roles));
    }
}
