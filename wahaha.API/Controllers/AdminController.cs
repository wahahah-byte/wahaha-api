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

    public AdminController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole(string email, string role)
    {
        var validRoles = new[] { WahahaUserRoles.Admin, WahahaUserRoles.Moderator, WahahaUserRoles.User };
        if (!validRoles.Contains(role))
            return BadRequest($"Invalid role. Valid roles are: {string.Join(", ", validRoles)}");

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound($"User with email {email} was not found.");

        if (await _userManager.IsInRoleAsync(user, role))
            return BadRequest($"User already has the {role} role.");

        var result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok($"Role '{role}' assigned to {email} successfully.");
    }

    [HttpPost("remove-role")]
    public async Task<IActionResult> RemoveRole(string email, string role)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound($"User with email {email} was not found.");

        if (!await _userManager.IsInRoleAsync(user, role))
            return BadRequest($"User does not have the {role} role.");

        // Prevent removing the last Admin
        if (role == WahahaUserRoles.Admin)
        {
            var admins = await _userManager.GetUsersInRoleAsync(WahahaUserRoles.Admin);
            if (admins.Count == 1)
                return BadRequest("Cannot remove the last Admin.");
        }

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok($"Role '{role}' removed from {email} successfully.");
    }

    [HttpGet("user-roles")]
    public async Task<IActionResult> GetUserRoles(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound($"User with email {email} was not found.");

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new { Email = email, Roles = roles });
    }
}
