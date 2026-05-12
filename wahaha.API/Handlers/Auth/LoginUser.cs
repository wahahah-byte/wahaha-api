using Microsoft.AspNetCore.Identity;
using wahaha.API.Data;
using wahaha.API.Models.Auth;
using wahaha.API.Models.DTOs;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Handlers.Auth;

public sealed record LoginUserRequest(LoginDto Dto);

public sealed class LoginUserHandler : IRequestHandler<LoginUserRequest, AuthResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly WahahaDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginUserHandler> _logger;

    public LoginUserHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        WahahaDbContext context,
        ITokenService tokenService,
        ILogger<LoginUserHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<HandlerResult<AuthResponseDto>> HandleAsync(LoginUserRequest request, CancellationToken ct = default)
    {
        var dto = request.Dto;
        _logger.LogInformation("Login attempt for email {Email}", dto.Email);

        var identityUser = await _userManager.FindByEmailAsync(dto.Email);
        if (identityUser == null)
        {
            _logger.LogWarning("Login failed — email {Email} not found", dto.Email);
            return HandlerResult<AuthResponseDto>.Unauthorized("Invalid email or password.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(identityUser, dto.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed — invalid password for {Email}", dto.Email);
            return HandlerResult<AuthResponseDto>.Unauthorized("Invalid email or password.");
        }

        var appUser = await _context.Users.FindAsync(new object?[] { identityUser.AppUserId }, ct);
        if (appUser == null)
        {
            _logger.LogError("Login failed — app user not found for identity user {IdentityId}", identityUser.Id);
            return HandlerResult<AuthResponseDto>.Unauthorized("User account not found.");
        }

        _logger.LogInformation("User {Username} logged in successfully", appUser.Username);
        var token = await _tokenService.CreateToken(identityUser, appUser.UserId, appUser.Username);
        return HandlerResult<AuthResponseDto>.Ok(new AuthResponseDto
        {
            Token = token,
            Expiry = DateTime.UtcNow.AddDays(7),
            UserId = appUser.UserId,
            Username = appUser.Username,
            Email = appUser.Email,
        });
    }
}
