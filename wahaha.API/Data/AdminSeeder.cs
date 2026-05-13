using Microsoft.AspNetCore.Identity;
using wahaha.API.Models.Auth;

namespace wahaha.API.Data;

// Runtime seeder that promotes one or more registered users to the Admin
// role based on configuration. Replaces the manual SQL INSERT into
// auth.AspNetUserRoles that was previously the only way to bootstrap an
// admin (since /api/Auth/register hardcodes the User role and the
// /api/Admin/assign-role endpoint is itself Admin-gated).
//
// Reads `Bootstrap:AdminEmail` from configuration — either a single email
// or a comma-separated list:
//
//   "Bootstrap": { "AdminEmail": "alice@example.com,bob@example.com" }
//
// For each email: looks up the user via UserManager.FindByEmailAsync.
//   - If the user doesn't exist (hasn't registered yet) → log warning, skip.
//   - If the user is already in Admin → log info, skip (idempotent across
//     restarts).
//   - Otherwise → AddToRoleAsync. Additive only; never demotes a previously
//     promoted account, even if that email is removed from the config later.
//
// Call this from Program.cs after app.Build() and before app.Run(). It
// creates its own DI scope (same shape as the dead-code RoleSeeder next to
// it) so it can resolve UserManager without polluting the request pipeline.
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
