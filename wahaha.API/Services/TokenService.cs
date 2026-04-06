using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using wahaha.API.Models.Auth;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<string> CreateToken(ApplicationUser user, Guid appUserId, string username)
    {
        _logger.LogDebug("Creating JWT token for user {Username} ({UserId})", username, appUserId);

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim("appUserId", appUserId.ToString()),
            new Claim("username", username)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiry = DateTime.UtcNow.AddDays(
            double.Parse(_configuration["Jwt:ExpiryDays"] ?? "7"));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: credentials
        );

        _logger.LogInformation("JWT token created for user {Username} with roles [{Roles}], expires {Expiry}",
            username, string.Join(", ", roles), expiry);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
