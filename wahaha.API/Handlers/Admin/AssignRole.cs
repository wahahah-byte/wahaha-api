using Microsoft.AspNetCore.Identity;
using wahaha.API.Models.Auth;

namespace wahaha.API.Handlers.Admin;

public sealed record AssignRoleRequest(string Email, string Role);

public sealed class AssignRoleHandler : IRequestHandler<AssignRoleRequest, string>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AssignRoleHandler> _logger;

    public AssignRoleHandler(UserManager<ApplicationUser> userManager, ILogger<AssignRoleHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<HandlerResult<string>> HandleAsync(AssignRoleRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Assigning role {Role} to {Email}", request.Role, request.Email);

        var validRoles = new[] { WahahaUserRoles.Admin, WahahaUserRoles.Moderator, WahahaUserRoles.User };
        if (!validRoles.Contains(request.Role))
        {
            _logger.LogWarning("Invalid role {Role} requested", request.Role);
            return HandlerResult<string>.BadRequest($"Invalid role. Valid roles are: {string.Join(", ", validRoles)}");
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("User {Email} not found for role assignment", request.Email);
            return HandlerResult<string>.NotFound($"User with email {request.Email} was not found.");
        }

        if (await _userManager.IsInRoleAsync(user, request.Role))
        {
            _logger.LogWarning("User {Email} already has role {Role}", request.Email, request.Role);
            return HandlerResult<string>.BadRequest($"User already has the {request.Role} role.");
        }

        var result = await _userManager.AddToRoleAsync(user, request.Role);
        if (!result.Succeeded)
        {
            var msg = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to assign role {Role} to {Email}: {Errors}", request.Role, request.Email, msg);
            return HandlerResult<string>.BadRequest(msg);
        }

        _logger.LogInformation("Role {Role} assigned to {Email} successfully", request.Role, request.Email);
        return HandlerResult<string>.Ok($"Role '{request.Role}' assigned to {request.Email} successfully.");
    }
}
