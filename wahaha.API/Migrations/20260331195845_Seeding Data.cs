using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "avatar_items",
                columns: table => new
                {
                    item_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    slot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rarity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cost = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    preview_asset_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_available = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avatar_items", x => x.item_id);
                });

            migrationBuilder.CreateTable(
                name: "minigames",
                columns: table => new
                {
                    game_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    max_points_reward = table.Column<int>(type: "int", nullable: false),
                    unlock_level = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    duration_seconds = table.Column<int>(type: "int", nullable: true),
                    difficulty = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_minigames", x => x.game_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    total_points_earned = table.Column<int>(type: "int", nullable: false),
                    current_balance = table.Column<int>(type: "int", nullable: false),
                    level = table.Column<int>(type: "int", nullable: false),
                    xp = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "minigame_sessions",
                columns: table => new
                {
                    session_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    game_id = table.Column<int>(type: "int", nullable: false),
                    score = table.Column<int>(type: "int", nullable: true),
                    points_earned = table.Column<int>(type: "int", nullable: false),
                    played_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    duration_seconds = table.Column<int>(type: "int", nullable: true),
                    outcome = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_minigame_sessions", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_minigame_sessions_minigames_game_id",
                        column: x => x.game_id,
                        principalTable: "minigames",
                        principalColumn: "game_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_minigame_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "point_transactions",
                columns: table => new
                {
                    transaction_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    amount = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    source_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    source_id = table.Column<int>(type: "int", nullable: true),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_point_transactions", x => x.transaction_id);
                    table.ForeignKey(
                        name: "FK_point_transactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "streaks",
                columns: table => new
                {
                    streak_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    streak_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    current_count = table.Column<int>(type: "int", nullable: false),
                    longest_count = table.Column<int>(type: "int", nullable: false),
                    last_activity_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    bonus_multiplier = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_streaks", x => x.streak_id);
                    table.ForeignKey(
                        name: "FK_streaks_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    task_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    point_value = table.Column<int>(type: "int", nullable: false),
                    due_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_recurring = table.Column<bool>(type: "bit", nullable: false),
                    recurrence_rule = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasks", x => x.task_id);
                    table.ForeignKey(
                        name: "FK_tasks_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_inventory",
                columns: table => new
                {
                    inventory_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    item_id = table.Column<int>(type: "int", nullable: false),
                    acquired_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_equipped = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_inventory", x => x.inventory_id);
                    table.ForeignKey(
                        name: "FK_user_inventory_avatar_items_item_id",
                        column: x => x.item_id,
                        principalTable: "avatar_items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_inventory_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "avatar_items",
                columns: new[] { "item_id", "category", "cost", "description", "is_available", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[,]
                {
                    { 1, "Hat", 50, "A simple starter cap.", true, "Rookie Cap", null, "COMMON", "HEAD" },
                    { 2, "Hat", 300, "A tall pointy hat for the wise.", true, "Wizard Hat", null, "RARE", "HEAD" },
                    { 3, "Hat", 2000, "Only the best earn this crown.", true, "Crown of Champions", null, "LEGENDARY", "HEAD" },
                    { 4, "Accessory", 40, "Classic round frames.", true, "Round Glasses", null, "COMMON", "FACE" },
                    { 5, "Accessory", 150, "8-bit style sunglasses.", true, "Pixel Shades", null, "UNCOMMON", "FACE" },
                    { 6, "Accessory", 800, "A sleek futuristic visor.", true, "Cyberpunk Visor", null, "EPIC", "FACE" },
                    { 7, "Shirt", 30, "A plain comfortable shirt.", true, "Basic Tee", null, "COMMON", "BODY" },
                    { 8, "Shirt", 120, "Cozy and casual.", true, "Hoodie", null, "UNCOMMON", "BODY" },
                    { 9, "Jacket", 500, "Stand out with this glowing jacket.", true, "Neon Jacket", null, "RARE", "BODY" },
                    { 10, "Armor", 3000, "Forged from dragon scales.", true, "Dragon Armor", null, "LEGENDARY", "BODY" },
                    { 11, "Gloves", 35, "Edgy and practical.", true, "Fingerless Gloves", null, "COMMON", "HAND" },
                    { 12, "Gloves", 900, "Feel unstoppable.", true, "Power Gauntlets", null, "EPIC", "HAND" },
                    { 13, "Back", 60, "A handy little pack.", true, "Mini Backpack", null, "COMMON", "BACK" },
                    { 14, "Back", 5000, "Ethereal wings of light.", true, "Angel Wings", null, "LEGENDARY", "BACK" },
                    { 15, "Shoes", 50, "Fresh kicks.", true, "Sneakers", null, "COMMON", "FEET" },
                    { 16, "Shoes", 1200, "Boots with built-in thrusters.", true, "Rocket Boots", null, "EPIC", "FEET" }
                });

            migrationBuilder.InsertData(
                table: "minigames",
                columns: new[] { "game_id", "description", "difficulty", "duration_seconds", "max_points_reward", "name", "type", "unlock_level" },
                values: new object[,]
                {
                    { 1, "Solve math problems as fast as you can.", "EASY", 60, 100, "Math Blitz", "quiz", 1 },
                    { 2, "Unscramble the letters to form words.", "EASY", 90, 120, "Word Scramble", "puzzle", 1 },
                    { 3, "Answer trivia questions before time runs out.", "MEDIUM", 120, 200, "Trivia Rush", "quiz", 3 },
                    { 4, "Navigate a platformer to reach the goal.", "MEDIUM", 180, 250, "Pixel Jumper", "platformer", 5 },
                    { 5, "Test your luck with coin flips.", "EASY", 30, 150, "Coin Flip Frenzy", "chance", 1 },
                    { 6, "Classic arcade-style action.", "HARD", 120, 300, "Arcade Fever", "arcade", 7 },
                    { 7, "Compete with others in a trivia showdown.", "HARD", 300, 400, "Team Trivia", "coop", 10 },
                    { 8, "Connect and challenge friends.", "EASY", 60, 180, "Social Shuffle", "social", 2 }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "user_id", "created_at", "current_balance", "email", "level", "total_points_earned", "username", "xp" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-0001-0001-0001-000000000001"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 800, "wizard@wahaha.com", 5, 1500, "byte_wizard", 4200 },
                    { new Guid("a1b2c3d4-0002-0002-0002-000000000002"), new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 300, "panda@wahaha.com", 3, 750, "pixel_panda", 1800 },
                    { new Guid("a1b2c3d4-0003-0003-0003-000000000003"), new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), 150, "nova@wahaha.com", 1, 200, "nova_runner", 400 }
                });

            migrationBuilder.InsertData(
                table: "point_transactions",
                columns: new[] { "transaction_id", "amount", "created_at", "description", "source_id", "source_type", "type", "user_id" },
                values: new object[,]
                {
                    { 1, 500, new DateTime(2025, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Task completion bonus", null, "task", "EARN", new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { 2, 300, new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Minigame reward", null, "minigame", "EARN", new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { 3, 700, new DateTime(2025, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), "7-day streak bonus", null, "streak", "EARN", new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { 4, 300, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Purchased Wizard Hat", null, "shop_item", "SPEND", new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { 5, 200, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "First win achievement", null, "achievement", "BONUS", new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { 6, 200, new DateTime(2025, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Task completion bonus", null, "task", "EARN", new Guid("a1b2c3d4-0002-0002-0002-000000000002") },
                    { 7, 150, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Minigame reward", null, "minigame", "EARN", new Guid("a1b2c3d4-0002-0002-0002-000000000002") },
                    { 8, 50, new DateTime(2025, 2, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Purchased Sneakers", null, "shop_item", "SPEND", new Guid("a1b2c3d4-0002-0002-0002-000000000002") },
                    { 9, 400, new DateTime(2025, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Streak milestone bonus", null, "streak", "EARN", new Guid("a1b2c3d4-0002-0002-0002-000000000002") },
                    { 10, 100, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tutorial completion reward", null, "task", "EARN", new Guid("a1b2c3d4-0003-0003-0003-000000000003") },
                    { 11, 100, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Welcome bonus", null, "achievement", "BONUS", new Guid("a1b2c3d4-0003-0003-0003-000000000003") },
                    { 12, 50, new DateTime(2025, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Purchased Basic Tee", null, "shop_item", "SPEND", new Guid("a1b2c3d4-0003-0003-0003-000000000003") }
                });

            migrationBuilder.InsertData(
                table: "streaks",
                columns: new[] { "streak_id", "bonus_multiplier", "current_count", "is_active", "last_activity_date", "longest_count", "streak_type", "user_id" },
                values: new object[,]
                {
                    { 1, 1.8m, 14, true, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 21, "daily_login", new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { 2, 1.5m, 7, true, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 14, "daily_task", new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { 3, 1.2m, 3, true, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, "daily_login", new Guid("a1b2c3d4-0002-0002-0002-000000000002") },
                    { 4, 1.0m, 1, true, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "daily_login", new Guid("a1b2c3d4-0003-0003-0003-000000000003") }
                });

            migrationBuilder.InsertData(
                table: "tasks",
                columns: new[] { "task_id", "category", "completed_at", "created_at", "description", "due_date", "is_recurring", "point_value", "priority", "recurrence_rule", "status", "title", "user_id" },
                values: new object[,]
                {
                    { new Guid("b0000001-0000-0000-0000-000000000001"), "Learning", new DateTime(2025, 3, 1, 12, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Finish the daily trivia quiz.", null, true, 50, "HIGH", "daily", "completed", "Complete daily quiz", new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000001-0000-0000-0000-000000000002"), "Gaming", null, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Win any 3 minigames this week.", null, false, 100, "MEDIUM", null, "in_progress", "Win 3 minigames", new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000001-0000-0000-0000-000000000003"), "Engagement", null, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Log in 7 days in a row.", new DateTime(2025, 3, 8, 0, 0, 0, 0, DateTimeKind.Utc), false, 150, "HIGH", null, "pending", "Maintain 7-day streak", new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000002-0000-0000-0000-000000000001"), "Shopping", new DateTime(2025, 2, 11, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 2, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Purchase any item from the shop.", null, false, 30, "LOW", null, "completed", "Buy first avatar item", new Guid("a1b2c3d4-0002-0002-0002-000000000002") },
                    { new Guid("b0000002-0000-0000-0000-000000000002"), "Gaming", null, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Join a cooperative minigame session.", new DateTime(2025, 3, 15, 0, 0, 0, 0, DateTimeKind.Utc), false, 75, "MEDIUM", null, "pending", "Play a coop game", new Guid("a1b2c3d4-0002-0002-0002-000000000002") },
                    { new Guid("b0000003-0000-0000-0000-000000000001"), "Onboarding", new DateTime(2025, 2, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Finish the onboarding tutorial.", null, false, 50, "CRITICAL", null, "completed", "Complete tutorial", new Guid("a1b2c3d4-0003-0003-0003-000000000003") },
                    { new Guid("b0000003-0000-0000-0000-000000000002"), "Gaming", null, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Try out any minigame.", null, false, 25, "HIGH", null, "pending", "Play first minigame", new Guid("a1b2c3d4-0003-0003-0003-000000000003") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_minigame_sessions_game_id",
                table: "minigame_sessions",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_minigame_sessions_user_id",
                table: "minigame_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_point_transactions_user_id",
                table: "point_transactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_streaks_user_id",
                table: "streaks",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_user_id",
                table: "tasks",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_inventory_item_id",
                table: "user_inventory",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_inventory_user_id",
                table: "user_inventory",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "minigame_sessions");

            migrationBuilder.DropTable(
                name: "point_transactions");

            migrationBuilder.DropTable(
                name: "streaks");

            migrationBuilder.DropTable(
                name: "tasks");

            migrationBuilder.DropTable(
                name: "user_inventory");

            migrationBuilder.DropTable(
                name: "minigames");

            migrationBuilder.DropTable(
                name: "avatar_items");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
