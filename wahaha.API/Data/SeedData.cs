using Microsoft.EntityFrameworkCore;
using wahaha.API.Models.Domain;

namespace wahaha.API.Data;

public static class SeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedAvatarItems(modelBuilder);
        SeedMinigames(modelBuilder);
        SeedUsers(modelBuilder);
        SeedUserInventory(modelBuilder);
        SeedTasks(modelBuilder);
        SeedPointTransactions(modelBuilder);
        SeedStreaks(modelBuilder);
    }

    // -------------------------------------------------------
    // Avatar Items
    // -------------------------------------------------------
    private static void SeedAvatarItems(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AvatarItem>().HasData(
            new AvatarItem { ItemId = 1, Name = "Wizard Hat", Category = "hat", Slot = ItemSlot.HEAD, Rarity = Rarity.RARE, Cost = 150, Description = "A pointy purple hat with golden stars", PreviewAssetUrl = "/assets/hats/hat_wizard.png", IsAvailable = true },
            new AvatarItem { ItemId = 2, Name = "Space Suit", Category = "outfit", Slot = ItemSlot.BODY, Rarity = Rarity.EPIC, Cost = 500, Description = "Full astronaut gear with visor", PreviewAssetUrl = "/assets/outfits/suit_space.png", IsAvailable = true },
            new AvatarItem { ItemId = 3, Name = "Golden Sword", Category = "accessory", Slot = ItemSlot.HAND, Rarity = Rarity.LEGENDARY, Cost = 1200, Description = "A glowing golden blade of champions", PreviewAssetUrl = "/assets/accessories/sword_gold.png", IsAvailable = true },
            new AvatarItem { ItemId = 4, Name = "Bunny Ears", Category = "hat", Slot = ItemSlot.HEAD, Rarity = Rarity.COMMON, Cost = 40, Description = "Soft pink ears with white fluff", PreviewAssetUrl = "/assets/hats/ears_bunny.png", IsAvailable = true },
            new AvatarItem { ItemId = 5, Name = "Adventurer Cloak", Category = "outfit", Slot = ItemSlot.BODY, Rarity = Rarity.UNCOMMON, Cost = 80, Description = "A worn but trusty traveler's cloak", PreviewAssetUrl = "/assets/outfits/cloak_adventurer.png", IsAvailable = true },
            new AvatarItem { ItemId = 6, Name = "Pixel Shades", Category = "accessory", Slot = ItemSlot.FACE, Rarity = Rarity.COMMON, Cost = 25, Description = "Retro 8-bit style sunglasses", PreviewAssetUrl = "/assets/accessories/shades_pixel.png", IsAvailable = true },
            new AvatarItem { ItemId = 7, Name = "Dragon Wings", Category = "back", Slot = ItemSlot.BACK, Rarity = Rarity.LEGENDARY, Cost = 1500, Description = "Massive fire-breathing dragon wings", PreviewAssetUrl = "/assets/back/wings_dragon.png", IsAvailable = true },
            new AvatarItem { ItemId = 8, Name = "Flower Crown", Category = "hat", Slot = ItemSlot.HEAD, Rarity = Rarity.COMMON, Cost = 35, Description = "A delicate crown of wildflowers", PreviewAssetUrl = "/assets/hats/crown_flower.png", IsAvailable = true },
            new AvatarItem { ItemId = 9, Name = "Ninja Outfit", Category = "outfit", Slot = ItemSlot.BODY, Rarity = Rarity.RARE, Cost = 200, Description = "Stealthy all-black ninja attire", PreviewAssetUrl = "/assets/outfits/outfit_ninja.png", IsAvailable = true },
            new AvatarItem { ItemId = 10, Name = "Magic Staff", Category = "accessory", Slot = ItemSlot.HAND, Rarity = Rarity.EPIC, Cost = 600, Description = "A glowing staff crackling with energy", PreviewAssetUrl = "/assets/accessories/staff_magic.png", IsAvailable = true },
            new AvatarItem { ItemId = 11, Name = "Cape of Valor", Category = "back", Slot = ItemSlot.BACK, Rarity = Rarity.UNCOMMON, Cost = 120, Description = "A red cape that billows heroically", PreviewAssetUrl = "/assets/back/cape_valor.png", IsAvailable = true },
            new AvatarItem { ItemId = 12, Name = "Robot Helmet", Category = "hat", Slot = ItemSlot.HEAD, Rarity = Rarity.EPIC, Cost = 450, Description = "A sleek metallic robot head piece", PreviewAssetUrl = "/assets/hats/helmet_robot.png", IsAvailable = true },
            new AvatarItem { ItemId = 13, Name = "Casual Hoodie", Category = "outfit", Slot = ItemSlot.BODY, Rarity = Rarity.COMMON, Cost = 30, Description = "A comfy everyday hoodie", PreviewAssetUrl = "/assets/outfits/hoodie_casual.png", IsAvailable = true },
            new AvatarItem { ItemId = 14, Name = "Pirate Hat", Category = "hat", Slot = ItemSlot.HEAD, Rarity = Rarity.UNCOMMON, Cost = 90, Description = "A classic tricorn pirate hat", PreviewAssetUrl = "/assets/hats/hat_pirate.png", IsAvailable = true },
            new AvatarItem { ItemId = 15, Name = "Shield of Tasks", Category = "accessory", Slot = ItemSlot.HAND, Rarity = Rarity.RARE, Cost = 180, Description = "A shield engraved with completed quests", PreviewAssetUrl = "/assets/accessories/shield_tasks.png", IsAvailable = true }
        );
    }

    // -------------------------------------------------------
    // Minigames
    // -------------------------------------------------------
    private static void SeedMinigames(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Minigame>().HasData(
            new Minigame { GameId = 1, Name = "Math Blitz", Description = "Solve math problems as fast as you can.", MaxPointsReward = 100, UnlockLevel = 1, Type = GameType.quiz, DurationSeconds = 60, Difficulty = Difficulty.EASY },
            new Minigame { GameId = 2, Name = "Word Scramble", Description = "Unscramble the letters to form words.", MaxPointsReward = 120, UnlockLevel = 1, Type = GameType.puzzle, DurationSeconds = 90, Difficulty = Difficulty.EASY },
            new Minigame { GameId = 3, Name = "Trivia Rush", Description = "Answer trivia questions before time runs out.", MaxPointsReward = 200, UnlockLevel = 3, Type = GameType.quiz, DurationSeconds = 120, Difficulty = Difficulty.MEDIUM },
            new Minigame { GameId = 4, Name = "Pixel Jumper", Description = "Navigate a platformer to reach the goal.", MaxPointsReward = 250, UnlockLevel = 5, Type = GameType.platformer, DurationSeconds = 180, Difficulty = Difficulty.MEDIUM },
            new Minigame { GameId = 5, Name = "Coin Flip Frenzy", Description = "Test your luck with coin flips.", MaxPointsReward = 150, UnlockLevel = 1, Type = GameType.chance, DurationSeconds = 30, Difficulty = Difficulty.EASY },
            new Minigame { GameId = 6, Name = "Arcade Fever", Description = "Classic arcade-style action.", MaxPointsReward = 300, UnlockLevel = 7, Type = GameType.arcade, DurationSeconds = 120, Difficulty = Difficulty.HARD },
            new Minigame { GameId = 7, Name = "Team Trivia", Description = "Compete with others in a trivia showdown.", MaxPointsReward = 400, UnlockLevel = 10, Type = GameType.coop, DurationSeconds = 300, Difficulty = Difficulty.HARD },
            new Minigame { GameId = 8, Name = "Social Shuffle", Description = "Connect and challenge friends.", MaxPointsReward = 180, UnlockLevel = 2, Type = GameType.social, DurationSeconds = 60, Difficulty = Difficulty.EASY }
        );
    }

    // -------------------------------------------------------
    // Users — emails follow username+test@wahaha.com format
    // -------------------------------------------------------
    private static readonly Guid User1Id = new("ea80e4bc-017d-4729-99d3-29ad8a5d9b47"); // DailyDragon
    private static readonly Guid User2Id = new("9e8e1aa6-273c-48c3-8634-56b630d6f3e3"); // QuestKnight
    private static readonly Guid User3Id = new("d5b63d3e-6049-44ed-92df-69560148f8a3"); // ShadowFox
    private static readonly Guid User4Id = new("80b6c179-8747-4a29-b112-7d774871a435"); // LevelUpLila
    private static readonly Guid User5Id = new("8e43c4d2-8381-44c7-808c-8f4c98d8d57b"); // PixelQueen
    private static readonly Guid User6Id = new("60892566-964c-4cdd-b9d1-f067981cd271"); // TaskMaster9

    private static void SeedUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Users>().HasData(
            new Users { UserId = User1Id, Username = "DailyDragon", Email = "dailydragon+test@wahaha.com", TotalPointsEarned = 310, CurrentBalance = 310, Level = 4, Xp = 1200, CreatedAt = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
            new Users { UserId = User2Id, Username = "QuestKnight", Email = "questknight+test@wahaha.com", TotalPointsEarned = 820, CurrentBalance = 200, Level = 9, Xp = 3300, CreatedAt = new DateTime(2026, 1, 22, 0, 0, 0, DateTimeKind.Utc) },
            new Users { UserId = User3Id, Username = "ShadowFox", Email = "shadowfox+test@wahaha.com", TotalPointsEarned = 0, CurrentBalance = 0, Level = 1, Xp = 0, CreatedAt = new DateTime(2026, 3, 24, 0, 0, 0, DateTimeKind.Utc) },
            new Users { UserId = User4Id, Username = "LevelUpLila", Email = "leveluplila+test@wahaha.com", TotalPointsEarned = 90, CurrentBalance = 90, Level = 2, Xp = 300, CreatedAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Users { UserId = User5Id, Username = "PixelQueen", Email = "pixelqueen+test@wahaha.com", TotalPointsEarned = 1240, CurrentBalance = 380, Level = 12, Xp = 4800, CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
            new Users { UserId = User6Id, Username = "TaskMaster9", Email = "taskmaster9+test@wahaha.com", TotalPointsEarned = 560, CurrentBalance = 560, Level = 6, Xp = 2100, CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }

    // -------------------------------------------------------
    // User Inventory
    // -------------------------------------------------------
    private static void SeedUserInventory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserInventory>().HasData(
            new UserInventory { InventoryId = 1, UserId = User2Id, ItemId = 1, AcquiredAt = new DateTime(2026, 1, 25, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 2, UserId = User2Id, ItemId = 4, AcquiredAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc), IsEquipped = false },
            new UserInventory { InventoryId = 3, UserId = User2Id, ItemId = 9, AcquiredAt = new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 4, UserId = User2Id, ItemId = 15, AcquiredAt = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 5, UserId = User2Id, ItemId = 11, AcquiredAt = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 6, UserId = User2Id, ItemId = 6, AcquiredAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 7, UserId = User5Id, ItemId = 12, AcquiredAt = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 8, UserId = User5Id, ItemId = 2, AcquiredAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 9, UserId = User5Id, ItemId = 10, AcquiredAt = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 10, UserId = User5Id, ItemId = 7, AcquiredAt = new DateTime(2026, 1, 25, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 11, UserId = User5Id, ItemId = 6, AcquiredAt = new DateTime(2026, 1, 28, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 12, UserId = User5Id, ItemId = 3, AcquiredAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc), IsEquipped = false },
            new UserInventory { InventoryId = 13, UserId = User6Id, ItemId = 14, AcquiredAt = new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 14, UserId = User6Id, ItemId = 5, AcquiredAt = new DateTime(2026, 2, 8, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 15, UserId = User6Id, ItemId = 15, AcquiredAt = new DateTime(2026, 2, 12, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 16, UserId = User1Id, ItemId = 8, AcquiredAt = new DateTime(2026, 2, 16, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 17, UserId = User1Id, ItemId = 13, AcquiredAt = new DateTime(2026, 2, 18, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 18, UserId = User1Id, ItemId = 11, AcquiredAt = new DateTime(2026, 2, 22, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 19, UserId = User4Id, ItemId = 4, AcquiredAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true },
            new UserInventory { InventoryId = 20, UserId = User4Id, ItemId = 13, AcquiredAt = new DateTime(2026, 3, 3, 0, 0, 0, DateTimeKind.Utc), IsEquipped = true }
        );
    }

    // -------------------------------------------------------
    // Tasks
    // -------------------------------------------------------
    private static void SeedTasks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.Domain.Task>().HasData(
            new Models.Domain.Task { TaskId = new Guid("b0000001-0000-0000-0000-000000000001"), UserId = User2Id, Title = "Complete daily quiz", Category = "Learning", Priority = Priority.HIGH, Status = ByteTaskStatus.completed, PointValue = 50, CreatedAt = new DateTime(2026, 1, 25, 0, 0, 0, DateTimeKind.Utc), CompletedAt = new DateTime(2026, 1, 25, 12, 0, 0, DateTimeKind.Utc), IsRecurring = true, RecurrenceRule = "daily" },
            new Models.Domain.Task { TaskId = new Guid("b0000001-0000-0000-0000-000000000002"), UserId = User2Id, Title = "Win 3 minigames", Category = "Gaming", Priority = Priority.MEDIUM, Status = ByteTaskStatus.in_progress, PointValue = 100, CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc), IsRecurring = false },
            new Models.Domain.Task { TaskId = new Guid("b0000002-0000-0000-0000-000000000001"), UserId = User5Id, Title = "Reach level 13", Category = "Progression", Priority = Priority.HIGH, Status = ByteTaskStatus.pending, PointValue = 200, CreatedAt = new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc), IsRecurring = false },
            new Models.Domain.Task { TaskId = new Guid("b0000002-0000-0000-0000-000000000002"), UserId = User5Id, Title = "Equip a legendary item", Category = "Collection", Priority = Priority.MEDIUM, Status = ByteTaskStatus.completed, PointValue = 75, CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), CompletedAt = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc), IsRecurring = false },
            new Models.Domain.Task { TaskId = new Guid("b0000003-0000-0000-0000-000000000001"), UserId = User6Id, Title = "Complete 5 tasks", Category = "Productivity", Priority = Priority.HIGH, Status = ByteTaskStatus.in_progress, PointValue = 120, CreatedAt = new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc), IsRecurring = false },
            new Models.Domain.Task { TaskId = new Guid("b0000004-0000-0000-0000-000000000001"), UserId = User1Id, Title = "Play a minigame", Category = "Gaming", Priority = Priority.LOW, Status = ByteTaskStatus.pending, PointValue = 25, CreatedAt = new DateTime(2026, 2, 16, 0, 0, 0, DateTimeKind.Utc), IsRecurring = false },
            new Models.Domain.Task { TaskId = new Guid("b0000005-0000-0000-0000-000000000001"), UserId = User4Id, Title = "Complete tutorial", Category = "Onboarding", Priority = Priority.CRITICAL, Status = ByteTaskStatus.completed, PointValue = 50, CreatedAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), CompletedAt = new DateTime(2026, 3, 1, 2, 0, 0, DateTimeKind.Utc), IsRecurring = false },
            new Models.Domain.Task { TaskId = new Guid("b0000006-0000-0000-0000-000000000001"), UserId = User3Id, Title = "Set up your profile", Category = "Onboarding", Priority = Priority.CRITICAL, Status = ByteTaskStatus.pending, PointValue = 30, CreatedAt = new DateTime(2026, 3, 24, 0, 0, 0, DateTimeKind.Utc), IsRecurring = false }
        );
    }

    // -------------------------------------------------------
    // Point Transactions
    // -------------------------------------------------------
    private static void SeedPointTransactions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PointTransaction>().HasData(
            new PointTransaction { TransactionId = 1, UserId = User2Id, Amount = 500, Type = TransactionType.EARN, SourceType = SourceType.task, Description = "Task completion bonus", CreatedAt = new DateTime(2026, 1, 25, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 2, UserId = User2Id, Amount = 300, Type = TransactionType.EARN, SourceType = SourceType.minigame, Description = "Minigame reward", CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 3, UserId = User2Id, Amount = 180, Type = TransactionType.SPEND, SourceType = SourceType.shop_item, Description = "Purchased Shield of Tasks", CreatedAt = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 4, UserId = User5Id, Amount = 600, Type = TransactionType.EARN, SourceType = SourceType.minigame, Description = "Minigame reward", CreatedAt = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 5, UserId = User5Id, Amount = 400, Type = TransactionType.EARN, SourceType = SourceType.streak, Description = "Streak milestone bonus", CreatedAt = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 6, UserId = User5Id, Amount = 600, Type = TransactionType.SPEND, SourceType = SourceType.shop_item, Description = "Purchased Magic Staff", CreatedAt = new DateTime(2026, 1, 22, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 7, UserId = User5Id, Amount = 200, Type = TransactionType.BONUS, SourceType = SourceType.achievement, Description = "Legendary item achievement", CreatedAt = new DateTime(2026, 1, 25, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 8, UserId = User6Id, Amount = 350, Type = TransactionType.EARN, SourceType = SourceType.task, Description = "Task completion bonus", CreatedAt = new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 9, UserId = User6Id, Amount = 210, Type = TransactionType.EARN, SourceType = SourceType.minigame, Description = "Minigame reward", CreatedAt = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 10, UserId = User1Id, Amount = 200, Type = TransactionType.EARN, SourceType = SourceType.task, Description = "Task completion bonus", CreatedAt = new DateTime(2026, 2, 18, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 11, UserId = User1Id, Amount = 110, Type = TransactionType.EARN, SourceType = SourceType.minigame, Description = "Minigame reward", CreatedAt = new DateTime(2026, 2, 22, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 12, UserId = User4Id, Amount = 50, Type = TransactionType.EARN, SourceType = SourceType.task, Description = "Tutorial completion reward", CreatedAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 13, UserId = User4Id, Amount = 40, Type = TransactionType.EARN, SourceType = SourceType.minigame, Description = "First minigame reward", CreatedAt = new DateTime(2026, 3, 3, 0, 0, 0, DateTimeKind.Utc) }
        );
    }

    // -------------------------------------------------------
    // Streaks
    // -------------------------------------------------------
    private static void SeedStreaks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Streak>().HasData(
            new Streak { StreakId = 1, UserId = User2Id, StreakType = "daily_login", CurrentCount = 14, LongestCount = 21, LastActivityDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), BonusMultiplier = 1.8m, IsActive = true },
            new Streak { StreakId = 2, UserId = User2Id, StreakType = "daily_task", CurrentCount = 7, LongestCount = 14, LastActivityDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), BonusMultiplier = 1.5m, IsActive = true },
            new Streak { StreakId = 3, UserId = User5Id, StreakType = "daily_login", CurrentCount = 30, LongestCount = 45, LastActivityDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), BonusMultiplier = 2.0m, IsActive = true },
            new Streak { StreakId = 4, UserId = User6Id, StreakType = "daily_login", CurrentCount = 5, LongestCount = 10, LastActivityDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), BonusMultiplier = 1.2m, IsActive = true },
            new Streak { StreakId = 5, UserId = User1Id, StreakType = "daily_login", CurrentCount = 3, LongestCount = 7, LastActivityDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), BonusMultiplier = 1.2m, IsActive = true },
            new Streak { StreakId = 6, UserId = User4Id, StreakType = "daily_login", CurrentCount = 1, LongestCount = 1, LastActivityDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), BonusMultiplier = 1.0m, IsActive = true },
            new Streak { StreakId = 7, UserId = User3Id, StreakType = "daily_login", CurrentCount = 0, LongestCount = 0, LastActivityDate = new DateTime(2026, 3, 24, 0, 0, 0, DateTimeKind.Utc), BonusMultiplier = 1.0m, IsActive = false }
        );
    }
}