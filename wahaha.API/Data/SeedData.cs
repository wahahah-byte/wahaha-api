using Microsoft.EntityFrameworkCore;
using wahaha.API.Models.Domain;
using Task = wahaha.API.Models.Domain.Task;

namespace wahaha.API.Data;

public static class SeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedAvatarItems(modelBuilder);
        SeedMinigames(modelBuilder);
        SeedUsers(modelBuilder);
        SeedTasks(modelBuilder);
        SeedPointTransactions(modelBuilder);
        SeedStreaks(modelBuilder);
    }

    private static void SeedAvatarItems(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AvatarItem>().HasData(
            // HEAD
            new AvatarItem { ItemId = 1, Name = "Rookie Cap", Category = "Hat", Slot = ItemSlot.HEAD, Rarity = Rarity.COMMON, Cost = 50, Description = "A simple starter cap.", IsAvailable = true },
            new AvatarItem { ItemId = 2, Name = "Wizard Hat", Category = "Hat", Slot = ItemSlot.HEAD, Rarity = Rarity.RARE, Cost = 300, Description = "A tall pointy hat for the wise.", IsAvailable = true },
            new AvatarItem { ItemId = 3, Name = "Crown of Champions", Category = "Hat", Slot = ItemSlot.HEAD, Rarity = Rarity.LEGENDARY, Cost = 2000, Description = "Only the best earn this crown.", IsAvailable = true },

            // FACE
            new AvatarItem { ItemId = 4, Name = "Round Glasses", Category = "Accessory", Slot = ItemSlot.FACE, Rarity = Rarity.COMMON, Cost = 40, Description = "Classic round frames.", IsAvailable = true },
            new AvatarItem { ItemId = 5, Name = "Pixel Shades", Category = "Accessory", Slot = ItemSlot.FACE, Rarity = Rarity.UNCOMMON, Cost = 150, Description = "8-bit style sunglasses.", IsAvailable = true },
            new AvatarItem { ItemId = 6, Name = "Cyberpunk Visor", Category = "Accessory", Slot = ItemSlot.FACE, Rarity = Rarity.EPIC, Cost = 800, Description = "A sleek futuristic visor.", IsAvailable = true },

            // BODY
            new AvatarItem { ItemId = 7, Name = "Basic Tee", Category = "Shirt", Slot = ItemSlot.BODY, Rarity = Rarity.COMMON, Cost = 30, Description = "A plain comfortable shirt.", IsAvailable = true },
            new AvatarItem { ItemId = 8, Name = "Hoodie", Category = "Shirt", Slot = ItemSlot.BODY, Rarity = Rarity.UNCOMMON, Cost = 120, Description = "Cozy and casual.", IsAvailable = true },
            new AvatarItem { ItemId = 9, Name = "Neon Jacket", Category = "Jacket", Slot = ItemSlot.BODY, Rarity = Rarity.RARE, Cost = 500, Description = "Stand out with this glowing jacket.", IsAvailable = true },
            new AvatarItem { ItemId = 10, Name = "Dragon Armor", Category = "Armor", Slot = ItemSlot.BODY, Rarity = Rarity.LEGENDARY, Cost = 3000, Description = "Forged from dragon scales.", IsAvailable = true },

            // HAND
            new AvatarItem { ItemId = 11, Name = "Fingerless Gloves", Category = "Gloves", Slot = ItemSlot.HAND, Rarity = Rarity.COMMON, Cost = 35, Description = "Edgy and practical.", IsAvailable = true },
            new AvatarItem { ItemId = 12, Name = "Power Gauntlets", Category = "Gloves", Slot = ItemSlot.HAND, Rarity = Rarity.EPIC, Cost = 900, Description = "Feel unstoppable.", IsAvailable = true },

            // BACK
            new AvatarItem { ItemId = 13, Name = "Mini Backpack", Category = "Back", Slot = ItemSlot.BACK, Rarity = Rarity.COMMON, Cost = 60, Description = "A handy little pack.", IsAvailable = true },
            new AvatarItem { ItemId = 14, Name = "Angel Wings", Category = "Back", Slot = ItemSlot.BACK, Rarity = Rarity.LEGENDARY, Cost = 5000, Description = "Ethereal wings of light.", IsAvailable = true },

            // FEET
            new AvatarItem { ItemId = 15, Name = "Sneakers", Category = "Shoes", Slot = ItemSlot.FEET, Rarity = Rarity.COMMON, Cost = 50, Description = "Fresh kicks.", IsAvailable = true },
            new AvatarItem { ItemId = 16, Name = "Rocket Boots", Category = "Shoes", Slot = ItemSlot.FEET, Rarity = Rarity.EPIC, Cost = 1200, Description = "Boots with built-in thrusters.", IsAvailable = true }
        );
    }

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

    private static readonly Guid User1Id = new("a1b2c3d4-0001-0001-0001-000000000001");
    private static readonly Guid User2Id = new("a1b2c3d4-0002-0002-0002-000000000002");
    private static readonly Guid User3Id = new("a1b2c3d4-0003-0003-0003-000000000003");

    private static void SeedUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Users>().HasData(
            new Users
            {
                UserId = User1Id,
                Username = "byte_wizard",
                Email = "wizard@wahaha.com",
                TotalPointsEarned = 1500,
                CurrentBalance = 800,
                Level = 5,
                Xp = 4200,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Users
            {
                UserId = User2Id,
                Username = "pixel_panda",
                Email = "panda@wahaha.com",
                TotalPointsEarned = 750,
                CurrentBalance = 300,
                Level = 3,
                Xp = 1800,
                CreatedAt = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new Users
            {
                UserId = User3Id,
                Username = "nova_runner",
                Email = "nova@wahaha.com",
                TotalPointsEarned = 200,
                CurrentBalance = 150,
                Level = 1,
                Xp = 400,
                CreatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }

    private static void SeedTasks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Task>().HasData(
            // byte_wizard tasks
            new Task { TaskId = new Guid("b0000001-0000-0000-0000-000000000001"), UserId = User1Id, Title = "Complete daily quiz", Description = "Finish the daily trivia quiz.", Category = "Learning", Priority = Priority.HIGH, Status = ByteTaskStatus.completed, PointValue = 50, CreatedAt = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc), CompletedAt = new DateTime(2025, 3, 1, 12, 0, 0, DateTimeKind.Utc), IsRecurring = true, RecurrenceRule = "daily" },
            new Task { TaskId = new Guid("b0000001-0000-0000-0000-000000000002"), UserId = User1Id, Title = "Win 3 minigames", Description = "Win any 3 minigames this week.", Category = "Gaming", Priority = Priority.MEDIUM, Status = ByteTaskStatus.in_progress, PointValue = 100, CreatedAt = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc), IsRecurring = false },
            new Task { TaskId = new Guid("b0000001-0000-0000-0000-000000000003"), UserId = User1Id, Title = "Maintain 7-day streak", Description = "Log in 7 days in a row.", Category = "Engagement", Priority = Priority.HIGH, Status = ByteTaskStatus.pending, PointValue = 150, DueDate = new DateTime(2025, 3, 8, 0, 0, 0, DateTimeKind.Utc), CreatedAt = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc), IsRecurring = false },

            // pixel_panda tasks
            new Task { TaskId = new Guid("b0000002-0000-0000-0000-000000000001"), UserId = User2Id, Title = "Buy first avatar item", Description = "Purchase any item from the shop.", Category = "Shopping", Priority = Priority.LOW, Status = ByteTaskStatus.completed, PointValue = 30, CreatedAt = new DateTime(2025, 2, 10, 0, 0, 0, DateTimeKind.Utc), CompletedAt = new DateTime(2025, 2, 11, 0, 0, 0, DateTimeKind.Utc), IsRecurring = false },
            new Task { TaskId = new Guid("b0000002-0000-0000-0000-000000000002"), UserId = User2Id, Title = "Play a coop game", Description = "Join a cooperative minigame session.", Category = "Gaming", Priority = Priority.MEDIUM, Status = ByteTaskStatus.pending, PointValue = 75, DueDate = new DateTime(2025, 3, 15, 0, 0, 0, DateTimeKind.Utc), CreatedAt = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc), IsRecurring = false },

            // nova_runner tasks
            new Task { TaskId = new Guid("b0000003-0000-0000-0000-000000000001"), UserId = User3Id, Title = "Complete tutorial", Description = "Finish the onboarding tutorial.", Category = "Onboarding", Priority = Priority.CRITICAL, Status = ByteTaskStatus.completed, PointValue = 50, CreatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc), CompletedAt = new DateTime(2025, 2, 1, 1, 0, 0, DateTimeKind.Utc), IsRecurring = false },
            new Task { TaskId = new Guid("b0000003-0000-0000-0000-000000000002"), UserId = User3Id, Title = "Play first minigame", Description = "Try out any minigame.", Category = "Gaming", Priority = Priority.HIGH, Status = ByteTaskStatus.pending, PointValue = 25, CreatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc), IsRecurring = false }
        );
    }
    private static void SeedPointTransactions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PointTransaction>().HasData(
            // byte_wizard transactions
            new PointTransaction { TransactionId = 1, UserId = User1Id, Amount = 500, Type = TransactionType.EARN, SourceType = SourceType.task, Description = "Task completion bonus", CreatedAt = new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 2, UserId = User1Id, Amount = 300, Type = TransactionType.EARN, SourceType = SourceType.minigame, Description = "Minigame reward", CreatedAt = new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 3, UserId = User1Id, Amount = 700, Type = TransactionType.EARN, SourceType = SourceType.streak, Description = "7-day streak bonus", CreatedAt = new DateTime(2025, 1, 20, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 4, UserId = User1Id, Amount = 300, Type = TransactionType.SPEND, SourceType = SourceType.shop_item, Description = "Purchased Wizard Hat", CreatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 5, UserId = User1Id, Amount = 200, Type = TransactionType.BONUS, SourceType = SourceType.achievement, Description = "First win achievement", CreatedAt = new DateTime(2025, 2, 15, 0, 0, 0, DateTimeKind.Utc) },

            // pixel_panda transactions
            new PointTransaction { TransactionId = 6, UserId = User2Id, Amount = 200, Type = TransactionType.EARN, SourceType = SourceType.task, Description = "Task completion bonus", CreatedAt = new DateTime(2025, 1, 20, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 7, UserId = User2Id, Amount = 150, Type = TransactionType.EARN, SourceType = SourceType.minigame, Description = "Minigame reward", CreatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 8, UserId = User2Id, Amount = 50, Type = TransactionType.SPEND, SourceType = SourceType.shop_item, Description = "Purchased Sneakers", CreatedAt = new DateTime(2025, 2, 11, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 9, UserId = User2Id, Amount = 400, Type = TransactionType.EARN, SourceType = SourceType.streak, Description = "Streak milestone bonus", CreatedAt = new DateTime(2025, 2, 20, 0, 0, 0, DateTimeKind.Utc) },

            // nova_runner transactions
            new PointTransaction { TransactionId = 10, UserId = User3Id, Amount = 100, Type = TransactionType.EARN, SourceType = SourceType.task, Description = "Tutorial completion reward", CreatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 11, UserId = User3Id, Amount = 100, Type = TransactionType.BONUS, SourceType = SourceType.achievement, Description = "Welcome bonus", CreatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PointTransaction { TransactionId = 12, UserId = User3Id, Amount = 50, Type = TransactionType.SPEND, SourceType = SourceType.shop_item, Description = "Purchased Basic Tee", CreatedAt = new DateTime(2025, 2, 5, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
    private static void SeedStreaks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Streak>().HasData(
            // byte_wizard
            new Streak { StreakId = 1, UserId = User1Id, StreakType = "daily_login", CurrentCount = 14, LongestCount = 21, LastActivityDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc), BonusMultiplier = 1.8m, IsActive = true },
            new Streak { StreakId = 2, UserId = User1Id, StreakType = "daily_task", CurrentCount = 7, LongestCount = 14, LastActivityDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc), BonusMultiplier = 1.5m, IsActive = true },

            // pixel_panda
            new Streak { StreakId = 3, UserId = User2Id, StreakType = "daily_login", CurrentCount = 3, LongestCount = 10, LastActivityDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc), BonusMultiplier = 1.2m, IsActive = true },

            // nova_runner
            new Streak { StreakId = 4, UserId = User3Id, StreakType = "daily_login", CurrentCount = 1, LongestCount = 1, LastActivityDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc), BonusMultiplier = 1.0m, IsActive = true }
        );
    }
}