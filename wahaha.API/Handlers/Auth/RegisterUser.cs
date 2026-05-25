using Microsoft.AspNetCore.Identity;
using wahaha.API.Data;
using wahaha.API.Models.Auth;
using wahaha.API.Models.DTOs;
using wahaha.API.Services.Interfaces;
using DomainUsers = wahaha.API.Models.Domain.Users;

namespace wahaha.API.Handlers.Auth;

public sealed record RegisterUserRequest(RegisterDto Dto);

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserRequest, AuthResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly WahahaDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(
        UserManager<ApplicationUser> userManager,
        WahahaDbContext context,
        ITokenService tokenService,
        ILogger<RegisterUserHandler> logger)
    {
        _userManager = userManager;
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    // Minimum signup age (Children's Online Privacy Protection Act / Google Play general-audience policy).
    private const int MinimumAgeYears = 13;

    public async Task<HandlerResult<AuthResponseDto>> HandleAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        var dto = request.Dto;
        _logger.LogInformation("Registration attempt for email {Email}", dto.Email);

        // Age gate — reject implausible/future DOB AND under-13s. Computed against UTC today.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dto.DateOfBirth > today)
        {
            return HandlerResult<AuthResponseDto>.BadRequest("Date of birth cannot be in the future.");
        }
        var age = today.Year - dto.DateOfBirth.Year;
        if (dto.DateOfBirth > today.AddYears(-age)) age--;
        if (age < MinimumAgeYears)
        {
            _logger.LogWarning("Registration failed — applicant under {MinAge}", MinimumAgeYears);
            return HandlerResult<AuthResponseDto>.BadRequest($"You must be at least {MinimumAgeYears} years old to register.");
        }

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed — email {Email} already exists", dto.Email);
            return HandlerResult<AuthResponseDto>.BadRequest("An account with this email already exists.");
        }

        var usernameTaken = _context.Users.Any(u => u.Username == dto.Username);
        if (usernameTaken)
        {
            _logger.LogWarning("Registration failed — username {Username} already taken", dto.Username);
            return HandlerResult<AuthResponseDto>.BadRequest("Username is already taken.");
        }

        var appUser = new DomainUsers
        {
            UserId = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email,
            DateOfBirth = dto.DateOfBirth,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(appUser);
        await _context.SaveChangesAsync(ct);

        var identityUser = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            AppUserId = appUser.UserId
        };

        var result = await _userManager.CreateAsync(identityUser, dto.Password);
        if (!result.Succeeded)
        {
            var msg = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Registration failed for {Email}: {Errors}", dto.Email, msg);
            _context.Users.Remove(appUser);
            await _context.SaveChangesAsync(ct);
            return HandlerResult<AuthResponseDto>.BadRequest(msg);
        }

        await _userManager.AddToRoleAsync(identityUser, WahahaUserRoles.User);
        _logger.LogInformation("User {Username} registered successfully with ID {UserId}", dto.Username, appUser.UserId);

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
