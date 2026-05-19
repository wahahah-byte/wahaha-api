using wahaha.API.Models.Auth;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;
using wahaha.API.Models.DTOs;
using wahaha.API.Models.Pagination;

namespace wahaha.API.Handlers;

/// <summary>
/// One <c>AddScoped&lt;IRequestHandler&lt;Req, Res&gt;, Handler&gt;()</c> per
/// endpoint. Grouped by feature for readability — when adding a new endpoint,
/// drop the registration into the matching block.
/// </summary>
public static class HandlerRegistration
{
    public static IServiceCollection AddRequestHandlers(this IServiceCollection services)
    {
        // Admin
        services.AddScoped<IRequestHandler<Admin.AssignRoleRequest, string>, Admin.AssignRoleHandler>();
        services.AddScoped<IRequestHandler<Admin.RemoveRoleRequest, string>, Admin.RemoveRoleHandler>();
        services.AddScoped<IRequestHandler<Admin.GetUserRolesRequest, Admin.UserRolesDto>, Admin.GetUserRolesHandler>();

        // Auth
        services.AddScoped<IRequestHandler<Auth.RegisterUserRequest, AuthResponseDto>, Auth.RegisterUserHandler>();
        services.AddScoped<IRequestHandler<Auth.LoginUserRequest, AuthResponseDto>, Auth.LoginUserHandler>();

        // Avatar items
        services.AddScoped<IRequestHandler<AvatarItems.GetAvatarItemListRequest, PagedResult<AvatarItemDto>>, AvatarItems.GetAvatarItemListHandler>();
        services.AddScoped<IRequestHandler<AvatarItems.GetAvatarItemByIdRequest, AvatarItemDto>, AvatarItems.GetAvatarItemByIdHandler>();
        services.AddScoped<IRequestHandler<AvatarItems.GetAvatarItemsByRarityRequest, IEnumerable<AvatarItemDto>>, AvatarItems.GetAvatarItemsByRarityHandler>();
        services.AddScoped<IRequestHandler<AvatarItems.GetAvatarItemsBySlotRequest, IEnumerable<AvatarItemDto>>, AvatarItems.GetAvatarItemsBySlotHandler>();
        services.AddScoped<IRequestHandler<AvatarItems.CreateAvatarItemRequest, AvatarItemDto>, AvatarItems.CreateAvatarItemHandler>();
        services.AddScoped<IRequestHandler<AvatarItems.RegisterAvatarItemByUrlRequest, AvatarItemDto>, AvatarItems.RegisterAvatarItemByUrlHandler>();
        services.AddScoped<IRequestHandler<AvatarItems.UpdateAvatarItemRequest, Unit>, AvatarItems.UpdateAvatarItemHandler>();
        services.AddScoped<IRequestHandler<AvatarItems.ToggleAvatarItemAvailabilityRequest, Unit>, AvatarItems.ToggleAvatarItemAvailabilityHandler>();
        services.AddScoped<IRequestHandler<AvatarItems.RecomputeAvatarItemBoundsRequest, AvatarItemDto>, AvatarItems.RecomputeAvatarItemBoundsHandler>();
        services.AddScoped<IRequestHandler<AvatarItems.GrantAvatarItemRequest, UserInventoryDto>, AvatarItems.GrantAvatarItemHandler>();
        services.AddScoped<IRequestHandler<AvatarItems.PurchaseAvatarItemRequest, PurchaseAvatarItemResponseDto>, AvatarItems.PurchaseAvatarItemHandler>();
        services.AddScoped<IRequestHandler<AvatarItems.DeleteAvatarItemRequest, Unit>, AvatarItems.DeleteAvatarItemHandler>();

        // Minigames
        services.AddScoped<IRequestHandler<Minigames.GetMinigameListRequest, IEnumerable<MinigameDto>>, Minigames.GetMinigameListHandler>();
        services.AddScoped<IRequestHandler<Minigames.GetMinigameByIdRequest, MinigameDto>, Minigames.GetMinigameByIdHandler>();
        services.AddScoped<IRequestHandler<Minigames.GetUnlockedMinigamesRequest, IEnumerable<MinigameDto>>, Minigames.GetUnlockedMinigamesHandler>();
        services.AddScoped<IRequestHandler<Minigames.GetMinigamesByDifficultyRequest, IEnumerable<MinigameDto>>, Minigames.GetMinigamesByDifficultyHandler>();
        services.AddScoped<IRequestHandler<Minigames.CreateMinigameRequest, MinigameDto>, Minigames.CreateMinigameHandler>();
        services.AddScoped<IRequestHandler<Minigames.UpdateMinigameRequest, Unit>, Minigames.UpdateMinigameHandler>();
        services.AddScoped<IRequestHandler<Minigames.DeleteMinigameRequest, Unit>, Minigames.DeleteMinigameHandler>();

        // Minigame sessions
        services.AddScoped<IRequestHandler<MinigameSessions.GetSessionListRequest, IEnumerable<MinigameSessionDto>>, MinigameSessions.GetSessionListHandler>();
        services.AddScoped<IRequestHandler<MinigameSessions.GetSessionByIdRequest, MinigameSessionDto>, MinigameSessions.GetSessionByIdHandler>();
        services.AddScoped<IRequestHandler<MinigameSessions.GetSessionsByGameRequest, IEnumerable<MinigameSessionDto>>, MinigameSessions.GetSessionsByGameHandler>();
        services.AddScoped<IRequestHandler<MinigameSessions.GetGameLeaderboardRequest, IEnumerable<MinigameSessionLeaderboardDto>>, MinigameSessions.GetGameLeaderboardHandler>();
        services.AddScoped<IRequestHandler<MinigameSessions.CreateSessionRequest, MinigameSessionDto>, MinigameSessions.CreateSessionHandler>();
        services.AddScoped<IRequestHandler<MinigameSessions.DeleteSessionRequest, Unit>, MinigameSessions.DeleteSessionHandler>();

        // Points
        services.AddScoped<IRequestHandler<Points.SubmitPointsHandlerRequest, SubmitPointsResponse>, Points.SubmitPointsHandler>();

        // Point transactions
        services.AddScoped<IRequestHandler<PointTransactions.GetTransactionListRequest, PagedResult<PointTransactionDto>>, PointTransactions.GetTransactionListHandler>();
        services.AddScoped<IRequestHandler<PointTransactions.GetTransactionByIdRequest, PointTransactionDto>, PointTransactions.GetTransactionByIdHandler>();
        services.AddScoped<IRequestHandler<PointTransactions.GetTransactionsByTypeRequest, IEnumerable<PointTransactionDto>>, PointTransactions.GetTransactionsByTypeHandler>();
        services.AddScoped<IRequestHandler<PointTransactions.CreateTransactionRequest, PointTransactionDto>, PointTransactions.CreateTransactionHandler>();
        services.AddScoped<IRequestHandler<PointTransactions.DeleteTransactionRequest, Unit>, PointTransactions.DeleteTransactionHandler>();

        // Streaks
        services.AddScoped<IRequestHandler<Streaks.GetStreakListRequest, PagedResult<StreakDto>>, Streaks.GetStreakListHandler>();
        services.AddScoped<IRequestHandler<Streaks.GetStreakByIdRequest, StreakDto>, Streaks.GetStreakByIdHandler>();
        services.AddScoped<IRequestHandler<Streaks.GetActiveStreaksRequest, IEnumerable<StreakDto>>, Streaks.GetActiveStreaksHandler>();
        services.AddScoped<IRequestHandler<Streaks.CreateStreakRequest, StreakDto>, Streaks.CreateStreakHandler>();
        services.AddScoped<IRequestHandler<Streaks.UpdateStreakRequest, Unit>, Streaks.UpdateStreakHandler>();
        services.AddScoped<IRequestHandler<Streaks.IncrementStreakRequest, Unit>, Streaks.IncrementStreakHandler>();
        services.AddScoped<IRequestHandler<Streaks.ResetStreakRequest, Unit>, Streaks.ResetStreakHandler>();
        services.AddScoped<IRequestHandler<Streaks.DeleteStreakRequest, Unit>, Streaks.DeleteStreakHandler>();

        // Subtasks
        services.AddScoped<IRequestHandler<Subtasks.GetSubtasksForTaskRequest, IEnumerable<SubtaskDto>>, Subtasks.GetSubtasksForTaskHandler>();
        services.AddScoped<IRequestHandler<Subtasks.CreateSubtaskRequestModel, SubtaskDto>, Subtasks.CreateSubtaskHandler>();
        services.AddScoped<IRequestHandler<Subtasks.UpdateSubtaskRequestModel, SubtaskDto>, Subtasks.UpdateSubtaskHandler>();
        services.AddScoped<IRequestHandler<Subtasks.DeleteSubtaskRequest, Unit>, Subtasks.DeleteSubtaskHandler>();
        services.AddScoped<IRequestHandler<Subtasks.ReorderSubtasksRequestModel, Unit>, Subtasks.ReorderSubtasksHandler>();

        // Tasks
        services.AddScoped<IRequestHandler<Tasks.GetTaskListRequest, PagedResult<TaskDto>>, Tasks.GetTaskListHandler>();
        services.AddScoped<IRequestHandler<Tasks.GetTaskByIdRequest, TaskDto>, Tasks.GetTaskByIdHandler>();
        services.AddScoped<IRequestHandler<Tasks.GetPendingTasksRequest, IEnumerable<TaskDto>>, Tasks.GetPendingTasksHandler>();
        services.AddScoped<IRequestHandler<Tasks.CreateTaskRequest, TaskDto>, Tasks.CreateTaskHandler>();
        services.AddScoped<IRequestHandler<Tasks.UpdateTaskRequest, Unit>, Tasks.UpdateTaskHandler>();
        services.AddScoped<IRequestHandler<Tasks.StartTaskRequest, Unit>, Tasks.StartTaskHandler>();
        services.AddScoped<IRequestHandler<Tasks.CompleteTaskRequest, Unit>, Tasks.CompleteTaskHandler>();
        services.AddScoped<IRequestHandler<Tasks.CheckInTaskRequest, CheckInResponse>, Tasks.CheckInTaskHandler>();
        services.AddScoped<IRequestHandler<Tasks.SkipCycleRequest, Tasks.SkipCycleResponse>, Tasks.SkipCycleHandler>();
        services.AddScoped<IRequestHandler<Tasks.ArchiveTaskRequest, Unit>, Tasks.ArchiveTaskHandler>();
        services.AddScoped<IRequestHandler<Tasks.UnarchiveTaskRequest, Unit>, Tasks.UnarchiveTaskHandler>();
        services.AddScoped<IRequestHandler<Tasks.GetCheckInHistoryRequest, PagedResult<CheckInCycleDto>>, Tasks.GetCheckInHistoryHandler>();
        services.AddScoped<IRequestHandler<Tasks.LogCounterRequest, CheckInCycleDto>, Tasks.LogCounterHandler>();
        services.AddScoped<IRequestHandler<Tasks.UpdateCheckInCycleRequest, Unit>, Tasks.UpdateCheckInCycleHandler>();
        services.AddScoped<IRequestHandler<Tasks.UndoCheckInHandlerRequest, UndoCheckInResponse>, Tasks.UndoCheckInHandler>();
        services.AddScoped<IRequestHandler<Tasks.RepairCheckInRequest, Tasks.RepairCheckInResponse>, Tasks.RepairCheckInHandler>();
        services.AddScoped<IRequestHandler<Tasks.DeleteLogCycleRequest, Unit>, Tasks.DeleteLogCycleHandler>();
        services.AddScoped<IRequestHandler<Tasks.DeleteTaskRequest, Unit>, Tasks.DeleteTaskHandler>();

        // User inventory
        services.AddScoped<IRequestHandler<UserInventory.GetInventoryListRequest, PagedResult<UserInventoryDto>>, UserInventory.GetInventoryListHandler>();
        services.AddScoped<IRequestHandler<UserInventory.GetInventoryByIdRequest, UserInventoryDto>, UserInventory.GetInventoryByIdHandler>();
        services.AddScoped<IRequestHandler<UserInventory.GetEquippedInventoryRequest, IEnumerable<UserInventoryDto>>, UserInventory.GetEquippedInventoryHandler>();
        services.AddScoped<IRequestHandler<UserInventory.CreateInventoryEntryRequest, UserInventoryDto>, UserInventory.CreateInventoryEntryHandler>();
        services.AddScoped<IRequestHandler<UserInventory.EquipInventoryRequest, Unit>, UserInventory.EquipInventoryHandler>();
        services.AddScoped<IRequestHandler<UserInventory.UnequipInventoryRequest, Unit>, UserInventory.UnequipInventoryHandler>();
        services.AddScoped<IRequestHandler<UserInventory.SetInventoryPositionRequest, Unit>, UserInventory.SetInventoryPositionHandler>();
        services.AddScoped<IRequestHandler<UserInventory.DeleteInventoryEntryRequest, Unit>, UserInventory.DeleteInventoryEntryHandler>();

        // Users
        services.AddScoped<IRequestHandler<Users.GetCurrentUserRequest, UserDto>, Users.GetCurrentUserHandler>();
        services.AddScoped<IRequestHandler<Users.UpdateUserRequest, Unit>, Users.UpdateUserHandler>();
        services.AddScoped<IRequestHandler<Users.UploadProfilePictureRequest, UserDto>, Users.UploadProfilePictureHandler>();
        services.AddScoped<IRequestHandler<Users.DeleteProfilePictureRequest, Unit>, Users.DeleteProfilePictureHandler>();
        services.AddScoped<IRequestHandler<Users.AddPointsRequest, Unit>, Users.AddPointsHandler>();
        services.AddScoped<IRequestHandler<Users.SpendPointsRequest, Unit>, Users.SpendPointsHandler>();

        return services;
    }
}
