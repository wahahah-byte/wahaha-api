using Microsoft.AspNetCore.Identity;
using wahaha.API.Models.Auth;

namespace wahaha.API.Handlers.Admin;

public sealed record RemoveRoleRequest(string Email, string Role);

public sealed class RemoveRoleHandler : IRequestHandler<RemoveRoleRequest, string>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RemoveRoleHandler> _logger;

    public RemoveRoleHandler(UserManager<ApplicationUser> userManager, ILogger<RemoveRoleHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<HandlerResult<string>> HandleAsync(RemoveRoleRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Removing role {Role} from {Email}", request.Role, request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("User {Email} not found for role removal", request.Email);
            return HandlerResult<string>.NotFound($"User with email {request.Email} was not found.");
        }

        if (!await _userManager.IsInRoleAsync(user, request.Role))
        {
            _logger.LogWarning("User {Email} does not have role {Role}", request.Email, request.Role);
            return HandlerResult<string>.BadRequest($"User does not have the {request.Role} role.");
        }

        if (request.Role == WahahaUserRoles.Admin)
        {
            var admins = await _userManager.GetUsersInRoleAsync(WahahaUserRoles.Admin);
            if (admins.Count == 1)
            {
                _logger.LogWarning("Attempt to remove last Admin role from {Email}", request.Email);
                return HandlerResult<string>.BadRequest("Cannot remove the last Admin.");
            }
        }

        var result = await _userManager.RemoveFromRoleAsync(user, request.Role);
        if (!result.Succeeded)
        {
            var msg = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to remove role {Role} from {Email}: {Errors}", request.Role, request.Email, msg);
            return HandlerResult<string>.BadRequest(msg);
        }

        _logger.LogInformation("Role {Role} removed from {Email} successfully", request.Role, request.Email);
        return HandlerResult<string>.Ok($"Role '{request.Role}' removed from {request.Email} successfully.");
    }
}
