using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Data;
using wahaha.API.Models.Auth;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTOs;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly WahahaDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        WahahaDbContext context,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return BadRequest("An account with this email already exists.");

        var usernameTaken = _context.Users.Any(u => u.Username == dto.Username);
        if (usernameTaken)
            return BadRequest("Username is already taken.");

        var appUser = new Users
        {
            UserId = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(appUser);
        await _context.SaveChangesAsync();

        var identityUser = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            AppUserId = appUser.UserId
        };

        var result = await _userManager.CreateAsync(identityUser, dto.Password);

        if (!result.Succeeded)
        {
            _context.Users.Remove(appUser);
            await _context.SaveChangesAsync();
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        // Assign default User role on registration
        await _userManager.AddToRoleAsync(identityUser, WahahaUserRoles.User);

        var token = await _tokenService.CreateToken(identityUser, appUser.UserId, appUser.Username);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Expiry = DateTime.UtcNow.AddDays(7),
            UserId = appUser.UserId,
            Username = appUser.Username,
            Email = appUser.Email
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var identityUser = await _userManager.FindByEmailAsync(dto.Email);

        if (identityUser == null)
            return Unauthorized("Invalid email or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(identityUser, dto.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
            return Unauthorized("Invalid email or password.");

        var appUser = await _context.Users.FindAsync(identityUser.AppUserId);

        if (appUser == null)
            return Unauthorized("User account not found.");

        var token = await _tokenService.CreateToken(identityUser, appUser.UserId, appUser.Username);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Expiry = DateTime.UtcNow.AddDays(7),
            UserId = appUser.UserId,
            Username = appUser.Username,
            Email = appUser.Email
        });
    }
}
