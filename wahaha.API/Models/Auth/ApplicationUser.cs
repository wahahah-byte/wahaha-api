using Microsoft.AspNetCore.Identity;

namespace wahaha.API.Models.Auth;

public class ApplicationUser : IdentityUser
{
    // Links to the Users domain table via shared UserId
    public Guid AppUserId { get; set; }
}
