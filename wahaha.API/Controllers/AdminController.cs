using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Handlers;
using wahaha.API.Handlers.Admin;
using wahaha.API.Models.Auth;

namespace wahaha.API.Controllers;

[Authorize(Roles = WahahaUserRoles.Admin)]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IRequestHandler<AssignRoleRequest, string> _assignRole;
    private readonly IRequestHandler<RemoveRoleRequest, string> _removeRole;
    private readonly IRequestHandler<GetUserRolesRequest, UserRolesDto> _getUserRoles;

    public AdminController(
        IRequestHandler<AssignRoleRequest, string> assignRole,
        IRequestHandler<RemoveRoleRequest, string> removeRole,
        IRequestHandler<GetUserRolesRequest, UserRolesDto> getUserRoles)
    {
        _assignRole = assignRole;
        _removeRole = removeRole;
        _getUserRoles = getUserRoles;
    }

    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole(string email, string role)
        => (await _assignRole.HandleAsync(new AssignRoleRequest(email, role))).ToActionResult();

    [HttpPost("remove-role")]
    public async Task<IActionResult> RemoveRole(string email, string role)
        => (await _removeRole.HandleAsync(new RemoveRoleRequest(email, role))).ToActionResult();

    [HttpGet("user-roles")]
    public async Task<IActionResult> GetUserRoles(string email)
        => (await _getUserRoles.HandleAsync(new GetUserRolesRequest(email))).ToActionResult();
}
