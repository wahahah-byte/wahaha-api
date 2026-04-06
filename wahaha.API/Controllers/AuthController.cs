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
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        WahahaDbContext context,
        ITokenService tokenService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        _logger.LogInformation("Registration attempt for email {Email}", dto.Email);

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed — email {Email} already exists", dto.Email);
            return BadRequest("An account with this email already exists.");
        }

        var usernameTaken = _context.Users.Any(u => u.Username == dto.Username);
        if (usernameTaken)
        {
            _logger.LogWarning("Registration failed — username {Username} already taken", dto.Username);
            return BadRequest("Username is already taken.");
        }

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
            _logger.LogError("Registration failed for {Email}: {Errors}",
                dto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));

            _context.Users.Remove(appUser);
            await _context.SaveChangesAsync();
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        await _userManager.AddToRoleAsync(identityUser, WahahaUserRoles.User);

        _logger.LogInformation("User {Username} registered successfully with ID {UserId}",
            dto.Username, appUser.UserId);

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
        _logger.LogInformation("Login attempt for email {Email}", dto.Email);

        var identityUser = await _userManager.FindByEmailAsync(dto.Email);

        if (identityUser == null)
        {
            _logger.LogWarning("Login failed — email {Email} not found", dto.Email);
            return Unauthorized("Invalid email or password.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(identityUser, dto.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed — invalid password for {Email}", dto.Email);
            return Unauthorized("Invalid email or password.");
        }

        var appUser = await _context.Users.FindAsync(identityUser.AppUserId);

        if (appUser == null)
        {
            _logger.LogError("Login failed — app user not found for identity user {IdentityId}", identityUser.Id);
            return Unauthorized("User account not found.");
        }

        _logger.LogInformation("User {Username} logged in successfully", appUser.Username);

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
