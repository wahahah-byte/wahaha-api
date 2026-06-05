using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Auth;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Handlers.Users;

public sealed record DeleteAccountRequest(Guid UserId);

// Hard-deletes the user, their domain rows, the profile-picture blob, and the Identity row for GDPR/CCPA compliance; transactional with best-effort blob delete.
public sealed class DeleteAccountHandler : IRequestHandler<DeleteAccountRequest, Unit>
{
    private readonly WahahaDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBlobService _blobService;
    private readonly ILogger<DeleteAccountHandler> _logger;
    private const string ProfilePictureContainer = "profile-pictures";

    public DeleteAccountHandler(
        WahahaDbContext context,
        UserManager<ApplicationUser> userManager,
        IBlobService blobService,
        ILogger<DeleteAccountHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _blobService = blobService;
        _logger = logger;
    }

    public async Task<HandlerResult<Unit>> HandleAsync(DeleteAccountRequest request, CancellationToken ct = default)
    {
        var userId = request.UserId;
        _logger.LogInformation("Account deletion requested for user {UserId}", userId);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId, ct);
        if (user == null) return HandlerResult<Unit>.NotFound("User was not found.");
        var profilePictureUrl = user.ProfilePictureUrl;

        // Single transaction so a mid-flight failure rolls back; never leave a half-deleted user.
        await using var tx = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            // Non-cascading rows deleted explicitly; Tasks → Subtasks + TaskCheckInCycles cascade per WahahaDbContext config.
            await _context.PointTransactions.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
            await _context.MinigameSessions.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
            await _context.Streaks.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
            await _context.UserInventories.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
            await _context.Tasks.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
            await _context.Users.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Account deletion transaction failed for {UserId}", userId);
            await tx.RollbackAsync(ct);
            return HandlerResult<Unit>.BadRequest("Account deletion failed — please try again.");
        }

        // Delete the Identity row separately (different DbContext); on failure the orphaned auth shell is logged loudly for cleanup.
        var identity = await _userManager.Users.FirstOrDefaultAsync(u => u.AppUserId == userId, ct);
        if (identity != null)
        {
            var result = await _userManager.DeleteAsync(identity);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to delete Identity row for {UserId}: {Errors}",
                    userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Best-effort blob cleanup — orphaned blob is not worth failing the call for.
        if (!string.IsNullOrEmpty(profilePictureUrl))
        {
            try { await _blobService.DeleteAsync(profilePictureUrl, ProfilePictureContainer); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete profile picture blob {Url}", profilePictureUrl); }
        }

        _logger.LogInformation("Account {UserId} deleted successfully", userId);
        return HandlerResult<Unit>.NoContent();
    }
}
