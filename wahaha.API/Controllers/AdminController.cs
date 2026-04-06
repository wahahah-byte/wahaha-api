using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Auth;

namespace wahaha.API.Controllers;

[Authorize(Roles = WahahaUserRoles.Admin)]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminController> _logger;

    public AdminController(UserManager<ApplicationUser> userManager, ILogger<AdminController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole(string email, string role)
    {
        _logger.LogInformation("Assigning role {Role} to {Email}", role, email);

        var validRoles = new[] { WahahaUserRoles.Admin, WahahaUserRoles.Moderator, WahahaUserRoles.User };
        if (!validRoles.Contains(role))
        {
            _logger.LogWarning("Invalid role {Role} requested", role);
            return BadRequest($"Invalid role. Valid roles are: {string.Join(", ", validRoles)}");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("User {Email} not found for role assignment", email);
            return NotFound($"User with email {email} was not found.");
        }

        if (await _userManager.IsInRoleAsync(user, role))
        {
            _logger.LogWarning("User {Email} already has role {Role}", email, role);
            return BadRequest($"User already has the {role} role.");
        }

        var result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to assign role {Role} to {Email}: {Errors}",
                role, email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        _logger.LogInformation("Role {Role} assigned to {Email} successfully", role, email);
        return Ok($"Role '{role}' assigned to {email} successfully.");
    }

    [HttpPost("remove-role")]
    public async Task<IActionResult> RemoveRole(string email, string role)
    {
        _logger.LogInformation("Removing role {Role} from {Email}", role, email);

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("User {Email} not found for role removal", email);
            return NotFound($"User with email {email} was not found.");
        }

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            _logger.LogWarning("User {Email} does not have role {Role}", email, role);
            return BadRequest($"User does not have the {role} role.");
        }

        if (role == WahahaUserRoles.Admin)
        {
            var admins = await _userManager.GetUsersInRoleAsync(WahahaUserRoles.Admin);
            if (admins.Count == 1)
            {
                _logger.LogWarning("Attempt to remove last Admin role from {Email}", email);
                return BadRequest("Cannot remove the last Admin.");
            }
        }

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to remove role {Role} from {Email}: {Errors}",
                role, email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        _logger.LogInformation("Role {Role} removed from {Email} successfully", role, email);
        return Ok($"Role '{role}' removed from {email} successfully.");
    }

    [HttpGet("user-roles")]
    public async Task<IActionResult> GetUserRoles(string email)
    {
        _logger.LogDebug("Fetching roles for {Email}", email);

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("User {Email} not found when fetching roles", email);
            return NotFound($"User with email {email} was not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new { Email = email, Roles = roles });
    }
}
