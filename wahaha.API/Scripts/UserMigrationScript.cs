using Microsoft.AspNetCore.Identity;
using wahaha.API.Data;
using wahaha.API.Models.Auth;

namespace wahaha.API.Scripts;

public static class UserMigrationScript
{
    /// <summary>
    /// Call this once from Program.cs after var app = builder.Build()
    /// to migrate existing Users table entries into ASP.NET Core Identity.
    /// Remove the call from Program.cs after running successfully.
    /// </summary>
    public static async Task MigrateExistingUsers(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var wahahaContext = scope.ServiceProvider.GetRequiredService<WahahaDbContext>();

        var existingUsers = wahahaContext.Users.ToList();

        foreach (var appUser in existingUsers)
        {
            // Generate email from username: e.g. DailyDragon -> dailydragon+test@wahaha.com
            var formattedEmail = $"{appUser.Username.ToLower()}+test@wahaha.com";

            // Update the email in the Users table to match
            appUser.Email = formattedEmail;

            // Skip if identity user already exists for this email
            var existing = await userManager.FindByEmailAsync(formattedEmail);
            if (existing != null)
            {
                Console.WriteLine($"Skipping {formattedEmail} — identity already exists.");
                continue;
            }

            var identityUser = new ApplicationUser
            {
                UserName = appUser.Username,   // preserve original username
                Email = formattedEmail,
                EmailConfirmed = true,
                AppUserId = appUser.UserId
            };

            var tempPassword = "Wahaha123!";
            var result = await userManager.CreateAsync(identityUser, tempPassword);

            if (result.Succeeded)
                Console.WriteLine($"Migrated: {appUser.Username} -> {formattedEmail}");
            else
                Console.WriteLine($"Failed: {appUser.Username} — {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Save the updated emails back to the Users table
        await wahahaContext.SaveChangesAsync();

        Console.WriteLine("Migration complete.");
    }
}