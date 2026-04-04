using wahaha.API.Models.Auth;

namespace wahaha.API.Services.Interfaces;

public interface ITokenService
{
    Task<string> CreateToken(ApplicationUser user, Guid appUserId, string username);
}
