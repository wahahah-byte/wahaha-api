using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using wahaha.API.Models.Auth;
namespace wahaha.API.Data;

public class AuthDbContext : IdentityDbContext<ApplicationUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("auth");

        var adminId = "6c181267-198e-4f13-82ce-f98dae3b292b";
        var modId = "b63a2149-ad02-408d-bea7-8fe32157286f";
        var userId = "dbe856d6-3740-4b4f-b3c2-b694e102a738";

        var roles = new List<IdentityRole>
        {
            new IdentityRole
            {
                Id = adminId,
                ConcurrencyStamp = adminId,
                Name = WahahaUserRoles.Admin,
                NormalizedName = WahahaUserRoles.Admin.ToUpper()
            },
            new IdentityRole
            {
                Id = modId,
                ConcurrencyStamp = modId,
                Name = WahahaUserRoles.Moderator,
                NormalizedName = WahahaUserRoles.Moderator.ToUpper()
            },
            new IdentityRole
            {
                Id = userId,
                ConcurrencyStamp = userId,
                Name = WahahaUserRoles.User,
                NormalizedName = WahahaUserRoles.User.ToUpper()
            }
        };

        modelBuilder.Entity<IdentityRole>().HasData(roles);
    }
}