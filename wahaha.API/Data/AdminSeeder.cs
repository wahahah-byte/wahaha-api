using Microsoft.AspNetCore.Identity;
using wahaha.API.Models.Auth;

namespace wahaha.API.Data;

// Runtime seeder that promotes users to Admin from Bootstrap:AdminEmail config (additive, idempotent).
public static class AdminSeeder
{
    public static async Task SeedAdminsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;
        var config = sp.GetRequiredService<IConfiguration>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");

        var raw = config["Bootstrap:AdminEmail"];
        if (string.IsNullOrWhiteSpace(raw))
        {
            logger.LogDebug("AdminSeeder: no Bootstrap:AdminEmail configured, skipping");
            return;
        }

        var emails = raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(e => e.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var email in emails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                logger.LogWarning(
                    "AdminSeeder: no registered user found for email {Email} — register via /api/Auth/register first, then restart",
                    email);
                continue;
            }

            if (await userManager.IsInRoleAsync(user, WahahaUserRoles.Admin))
            {
                logger.LogInformation("AdminSeeder: {Email} already has the Admin role", email);
                continue;
            }

            var result = await userManager.AddToRoleAsync(user, WahahaUserRoles.Admin);
            if (result.Succeeded)
            {
                logger.LogInformation("AdminSeeder: granted Admin role to {Email}", email);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError(
                    "AdminSeeder: failed to grant Admin to {Email} — {Errors}",
                    email, errors);
            }
        }
    }
}
