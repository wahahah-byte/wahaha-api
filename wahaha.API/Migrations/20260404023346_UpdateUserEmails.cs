using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000003-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("a1b2c3d4-0002-0002-0002-000000000002"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("a1b2c3d4-0001-0001-0001-000000000001"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("a1b2c3d4-0003-0003-0003-000000000003"));

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 1,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity" },
                values: new object[] { "hat", 150, "A pointy purple hat with golden stars", "Wizard Hat", "/assets/hats/hat_wizard.png", "RARE" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 2,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[] { "outfit", 500, "Full astronaut gear with visor", "Space Suit", "/assets/outfits/suit_space.png", "EPIC", "BODY" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 3,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "slot" },
                values: new object[] { "accessory", 1200, "A glowing golden blade of champions", "Golden Sword", "/assets/accessories/sword_gold.png", "HAND" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 4,
                columns: new[] { "category", "description", "name", "preview_asset_url", "slot" },
                values: new object[] { "hat", "Soft pink ears with white fluff", "Bunny Ears", "/assets/hats/ears_bunny.png", "HEAD" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 5,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "slot" },
                values: new object[] { "outfit", 80, "A worn but trusty traveler's cloak", "Adventurer Cloak", "/assets/outfits/cloak_adventurer.png", "BODY" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 6,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity" },
                values: new object[] { "accessory", 25, "Retro 8-bit style sunglasses", "Pixel Shades", "/assets/accessories/shades_pixel.png", "COMMON" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 7,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[] { "back", 1500, "Massive fire-breathing dragon wings", "Dragon Wings", "/assets/back/wings_dragon.png", "LEGENDARY", "BACK" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 8,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[] { "hat", 35, "A delicate crown of wildflowers", "Flower Crown", "/assets/hats/crown_flower.png", "COMMON", "HEAD" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 9,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url" },
                values: new object[] { "outfit", 200, "Stealthy all-black ninja attire", "Ninja Outfit", "/assets/outfits/outfit_ninja.png" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 10,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[] { "accessory", 600, "A glowing staff crackling with energy", "Magic Staff", "/assets/accessories/staff_magic.png", "EPIC", "HAND" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 11,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[] { "back", 120, "A red cape that billows heroically", "Cape of Valor", "/assets/back/cape_valor.png", "UNCOMMON", "BACK" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 12,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "slot" },
                values: new object[] { "hat", 450, "A sleek metallic robot head piece", "Robot Helmet", "/assets/hats/helmet_robot.png", "HEAD" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 13,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "slot" },
                values: new object[] { "outfit", 30, "A comfy everyday hoodie", "Casual Hoodie", "/assets/outfits/hoodie_casual.png", "BODY" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 14,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[] { "hat", 90, "A classic tricorn pirate hat", "Pirate Hat", "/assets/hats/hat_pirate.png", "UNCOMMON", "HEAD" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 15,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[] { "accessory", 180, "A shield engraved with completed quests", "Shield of Tasks", "/assets/accessories/shield_tasks.png", "RARE", "HAND" });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 1,
                columns: new[] { "created_at", "user_id" },
                values: new object[] { new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("9e8e1aa6-273c-48c3-8634-56b630d6f3e3") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 2,
                columns: new[] { "created_at", "user_id" },
                values: new object[] { new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("9e8e1aa6-273c-48c3-8634-56b630d6f3e3") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 3,
                columns: new[] { "amount", "created_at", "description", "source_type", "type", "user_id" },
                values: new object[] { 180, new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Purchased Shield of Tasks", "shop_item", "SPEND", new Guid("9e8e1aa6-273c-48c3-8634-56b630d6f3e3") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 4,
                columns: new[] { "amount", "created_at", "description", "source_type", "type", "user_id" },
                values: new object[] { 600, new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Minigame reward", "minigame", "EARN", new Guid("8e43c4d2-8381-44c7-808c-8f4c98d8d57b") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 5,
                columns: new[] { "amount", "created_at", "description", "source_type", "type", "user_id" },
                values: new object[] { 400, new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Streak milestone bonus", "streak", "EARN", new Guid("8e43c4d2-8381-44c7-808c-8f4c98d8d57b") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 6,
                columns: new[] { "amount", "created_at", "description", "source_type", "type", "user_id" },
                values: new object[] { 600, new DateTime(2026, 1, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Purchased Magic Staff", "shop_item", "SPEND", new Guid("8e43c4d2-8381-44c7-808c-8f4c98d8d57b") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 7,
                columns: new[] { "amount", "created_at", "description", "source_type", "type", "user_id" },
                values: new object[] { 200, new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Legendary item achievement", "achievement", "BONUS", new Guid("8e43c4d2-8381-44c7-808c-8f4c98d8d57b") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 8,
                columns: new[] { "amount", "created_at", "description", "source_type", "type", "user_id" },
                values: new object[] { 350, new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Task completion bonus", "task", "EARN", new Guid("60892566-964c-4cdd-b9d1-f067981cd271") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 9,
                columns: new[] { "amount", "created_at", "description", "source_type", "user_id" },
                values: new object[] { 210, new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Minigame reward", "minigame", new Guid("60892566-964c-4cdd-b9d1-f067981cd271") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 10,
                columns: new[] { "amount", "created_at", "description", "user_id" },
                values: new object[] { 200, new DateTime(2026, 2, 18, 0, 0, 0, 0, DateTimeKind.Utc), "Task completion bonus", new Guid("ea80e4bc-017d-4729-99d3-29ad8a5d9b47") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 11,
                columns: new[] { "amount", "created_at", "description", "source_type", "type", "user_id" },
                values: new object[] { 110, new DateTime(2026, 2, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Minigame reward", "minigame", "EARN", new Guid("ea80e4bc-017d-4729-99d3-29ad8a5d9b47") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 12,
                columns: new[] { "created_at", "description", "source_type", "type", "user_id" },
                values: new object[] { new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tutorial completion reward", "task", "EARN", new Guid("80b6c179-8747-4a29-b112-7d774871a435") });

            migrationBuilder.UpdateData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 1,
                columns: new[] { "last_activity_date", "user_id" },
                values: new object[] { new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("9e8e1aa6-273c-48c3-8634-56b630d6f3e3") });

            migrationBuilder.UpdateData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 2,
                columns: new[] { "last_activity_date", "user_id" },
                values: new object[] { new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("9e8e1aa6-273c-48c3-8634-56b630d6f3e3") });

            migrationBuilder.UpdateData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 3,
                columns: new[] { "bonus_multiplier", "current_count", "last_activity_date", "longest_count", "user_id" },
                values: new object[] { 2.0m, 30, new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 45, new Guid("8e43c4d2-8381-44c7-808c-8f4c98d8d57b") });

            migrationBuilder.UpdateData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 4,
                columns: new[] { "bonus_multiplier", "current_count", "last_activity_date", "longest_count", "user_id" },
                values: new object[] { 1.2m, 5, new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, new Guid("60892566-964c-4cdd-b9d1-f067981cd271") });

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000001"),
                columns: new[] { "completed_at", "created_at", "description", "user_id" },
                values: new object[] { new DateTime(2026, 1, 25, 12, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("9e8e1aa6-273c-48c3-8634-56b630d6f3e3") });

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000002"),
                columns: new[] { "created_at", "description", "user_id" },
                values: new object[] { new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("9e8e1aa6-273c-48c3-8634-56b630d6f3e3") });

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000001"),
                columns: new[] { "category", "completed_at", "created_at", "description", "point_value", "priority", "status", "title", "user_id" },
                values: new object[] { "Progression", null, new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, 200, "HIGH", "pending", "Reach level 13", new Guid("8e43c4d2-8381-44c7-808c-8f4c98d8d57b") });

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000002"),
                columns: new[] { "category", "completed_at", "created_at", "description", "due_date", "status", "title", "user_id" },
                values: new object[] { "Collection", new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "completed", "Equip a legendary item", new Guid("8e43c4d2-8381-44c7-808c-8f4c98d8d57b") });

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000003-0000-0000-0000-000000000001"),
                columns: new[] { "category", "completed_at", "created_at", "description", "point_value", "priority", "status", "title", "user_id" },
                values: new object[] { "Productivity", null, new DateTime(2026, 2, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, 120, "HIGH", "in_progress", "Complete 5 tasks", new Guid("60892566-964c-4cdd-b9d1-f067981cd271") });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "user_id", "created_at", "current_balance", "email", "level", "total_points_earned", "username", "xp" },
                values: new object[,]
                {
                    { new Guid("60892566-964c-4cdd-b9d1-f067981cd271"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), 560, "taskmaster9+test@wahaha.com", 6, 560, "TaskMaster9", 2100 },
                    { new Guid("80b6c179-8747-4a29-b112-7d774871a435"), new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 90, "leveluplila+test@wahaha.com", 2, 90, "LevelUpLila", 300 },
                    { new Guid("8e43c4d2-8381-44c7-808c-8f4c98d8d57b"), new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), 380, "pixelqueen+test@wahaha.com", 12, 1240, "PixelQueen", 4800 },
                    { new Guid("9e8e1aa6-273c-48c3-8634-56b630d6f3e3"), new DateTime(2026, 1, 22, 0, 0, 0, 0, DateTimeKind.Utc), 200, "questknight+test@wahaha.com", 9, 820, "QuestKnight", 3300 },
                    { new Guid("d5b63d3e-6049-44ed-92df-69560148f8a3"), new DateTime(2026, 3, 24, 0, 0, 0, 0, DateTimeKind.Utc), 0, "shadowfox+test@wahaha.com", 1, 0, "ShadowFox", 0 },
                    { new Guid("ea80e4bc-017d-4729-99d3-29ad8a5d9b47"), new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 310, "dailydragon+test@wahaha.com", 4, 310, "DailyDragon", 1200 }
                });

            migrationBuilder.InsertData(
                table: "point_transactions",
                columns: new[] { "transaction_id", "amount", "created_at", "description", "source_id", "source_type", "type", "user_id" },
                values: new object[] { 13, 40, new DateTime(2026, 3, 3, 0, 0, 0, 0, DateTimeKind.Utc), "First minigame reward", null, "minigame", "EARN", new Guid("80b6c179-8747-4a29-b112-7d774871a435") });

            migrationBuilder.InsertData(
                table: "streaks",
                columns: new[] { "streak_id", "bonus_multiplier", "current_count", "is_active", "last_activity_date", "longest_count", "streak_type", "user_id" },
                values: new object[,]
                {
                    { 5, 1.2m, 3, true, new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "daily_login", new Guid("ea80e4bc-017d-4729-99d3-29ad8a5d9b47") },
                    { 6, 1.0m, 1, true, new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "daily_login", new Guid("80b6c179-8747-4a29-b112-7d774871a435") },
                    { 7, 1.0m, 0, false, new DateTime(2026, 3, 24, 0, 0, 0, 0, DateTimeKind.Utc), 0, "daily_login", new Guid("d5b63d3e-6049-44ed-92df-69560148f8a3") }
                });

            migrationBuilder.InsertData(
                table: "tasks",
                columns: new[] { "task_id", "category", "completed_at", "created_at", "description", "due_date", "is_recurring", "point_value", "priority", "recurrence_rule", "status", "title", "user_id" },
                values: new object[,]
                {
                    { new Guid("b0000004-0000-0000-0000-000000000001"), "Gaming", null, new DateTime(2026, 2, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, 25, "LOW", null, "pending", "Play a minigame", new Guid("ea80e4bc-017d-4729-99d3-29ad8a5d9b47") },
                    { new Guid("b0000005-0000-0000-0000-000000000001"), "Onboarding", new DateTime(2026, 3, 1, 2, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, 50, "CRITICAL", null, "completed", "Complete tutorial", new Guid("80b6c179-8747-4a29-b112-7d774871a435") },
                    { new Guid("b0000006-0000-0000-0000-000000000001"), "Onboarding", null, new DateTime(2026, 3, 24, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, 30, "CRITICAL", null, "pending", "Set up your profile", new Guid("d5b63d3e-6049-44ed-92df-69560148f8a3") }
                });

            migrationBuilder.InsertData(
                table: "user_inventory",
                columns: new[] { "inventory_id", "acquired_at", "is_equipped", "item_id", "user_id" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, new Guid("9e8e1aa6-273c-48c3-8634-56b630d6f3e3") },
                    { 2, new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 4, new Guid("9e8e1aa6-273c-48c3-8634-56b630d6f3e3") },
                    { 3, new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, new Guid("9e8e1aa6-273c-48c3-8634-56b630d6f3e3") },
                    { 4, new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Utc), true, 15, new Guid("9e8e1aa6-273c-48c3-8634-56b630d6f3e3") },
                    { 5, new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, 11, new Guid("9e8e1aa6-273c-48c3-8634-56b630d6f3e3") },
                    { 6, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, 6, new Guid("9e8e1aa6-273c-48c3-8634-56b630d6f3e3") },
                    { 7, new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), true, 12, new Guid("8e43c4d2-8381-44c7-808c-8f4c98d8d57b") },
                    { 8, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, 2, new Guid("8e43c4d2-8381-44c7-808c-8f4c98d8d57b") },
                    { 9, new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, 10, new Guid("8e43c4d2-8381-44c7-808c-8f4c98d8d57b") },
                    { 10, new DateTime(2026, 1, 25, 0, 0, 0, 0, DateTimeKind.Utc), true, 7, new Guid("8e43c4d2-8381-44c7-808c-8f4c98d8d57b") },
                    { 11, new DateTime(2026, 1, 28, 0, 0, 0, 0, DateTimeKind.Utc), true, 6, new Guid("8e43c4d2-8381-44c7-808c-8f4c98d8d57b") },
                    { 12, new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 3, new Guid("8e43c4d2-8381-44c7-808c-8f4c98d8d57b") },
                    { 13, new DateTime(2026, 2, 3, 0, 0, 0, 0, DateTimeKind.Utc), true, 14, new Guid("60892566-964c-4cdd-b9d1-f067981cd271") },
                    { 14, new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc), true, 5, new Guid("60892566-964c-4cdd-b9d1-f067981cd271") },
                    { 15, new DateTime(2026, 2, 12, 0, 0, 0, 0, DateTimeKind.Utc), true, 15, new Guid("60892566-964c-4cdd-b9d1-f067981cd271") },
                    { 16, new DateTime(2026, 2, 16, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, new Guid("ea80e4bc-017d-4729-99d3-29ad8a5d9b47") },
                    { 17, new DateTime(2026, 2, 18, 0, 0, 0, 0, DateTimeKind.Utc), true, 13, new Guid("ea80e4bc-017d-4729-99d3-29ad8a5d9b47") },
                    { 18, new DateTime(2026, 2, 22, 0, 0, 0, 0, DateTimeKind.Utc), true, 11, new Guid("ea80e4bc-017d-4729-99d3-29ad8a5d9b47") },
                    { 19, new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 4, new Guid("80b6c179-8747-4a29-b112-7d774871a435") },
                    { 20, new DateTime(2026, 3, 3, 0, 0, 0, 0, DateTimeKind.Utc), true, 13, new Guid("80b6c179-8747-4a29-b112-7d774871a435") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000004-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000005-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000006-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("60892566-964c-4cdd-b9d1-f067981cd271"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("80b6c179-8747-4a29-b112-7d774871a435"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("8e43c4d2-8381-44c7-808c-8f4c98d8d57b"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("9e8e1aa6-273c-48c3-8634-56b630d6f3e3"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("d5b63d3e-6049-44ed-92df-69560148f8a3"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("ea80e4bc-017d-4729-99d3-29ad8a5d9b47"));

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 1,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity" },
                values: new object[] { "Hat", 50, "A simple starter cap.", "Rookie Cap", null, "COMMON" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 2,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[] { "Hat", 300, "A tall pointy hat for the wise.", "Wizard Hat", null, "RARE", "HEAD" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 3,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "slot" },
                values: new object[] { "Hat", 2000, "Only the best earn this crown.", "Crown of Champions", null, "HEAD" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 4,
                columns: new[] { "category", "description", "name", "preview_asset_url", "slot" },
                values: new object[] { "Accessory", "Classic round frames.", "Round Glasses", null, "FACE" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 5,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "slot" },
                values: new object[] { "Accessory", 150, "8-bit style sunglasses.", "Pixel Shades", null, "FACE" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 6,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity" },
                values: new object[] { "Accessory", 800, "A sleek futuristic visor.", "Cyberpunk Visor", null, "EPIC" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 7,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[] { "Shirt", 30, "A plain comfortable shirt.", "Basic Tee", null, "COMMON", "BODY" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 8,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[] { "Shirt", 120, "Cozy and casual.", "Hoodie", null, "UNCOMMON", "BODY" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 9,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url" },
                values: new object[] { "Jacket", 500, "Stand out with this glowing jacket.", "Neon Jacket", null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 10,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[] { "Armor", 3000, "Forged from dragon scales.", "Dragon Armor", null, "LEGENDARY", "BODY" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 11,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[] { "Gloves", 35, "Edgy and practical.", "Fingerless Gloves", null, "COMMON", "HAND" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 12,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "slot" },
                values: new object[] { "Gloves", 900, "Feel unstoppable.", "Power Gauntlets", null, "HAND" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 13,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "slot" },
                values: new object[] { "Back", 60, "A handy little pack.", "Mini Backpack", null, "BACK" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 14,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[] { "Back", 5000, "Ethereal wings of light.", "Angel Wings", null, "LEGENDARY", "BACK" });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 15,
                columns: new[] { "category", "cost", "description", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[] { "Shoes", 50, "Fresh kicks.", "Sneakers", null, "COMMON", "FEET" });

            migrationBuilder.InsertData(
                table: "avatar_items",
                columns: new[] { "item_id", "category", "cost", "description", "is_available", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[] { 16, "Shoes", 1200, "Boots with built-in thrusters.", true, "Rocket Boots", null, "EPIC", "FEET" });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 1,
                columns: new[] { "created_at", "user_id" },
                values: new object[] { new DateTime(2025, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 2,
                columns: new[] { "created_at", "user_id" },
                values: new object[] { new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 3,
                columns: new[] { "amount", "created_at", "description", "source_type", "type", "user_id" },
                values: new object[] { 700, new DateTime(2025, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), "7-day streak bonus", "streak", "EARN", new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 4,
                columns: new[] { "amount", "created_at", "description", "source_type", "type", "user_id" },
                values: new object[] { 300, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Purchased Wizard Hat", "shop_item", "SPEND", new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 5,
                columns: new[] { "amount", "created_at", "description", "source_type", "type", "user_id" },
                values: new object[] { 200, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "First win achievement", "achievement", "BONUS", new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 6,
                columns: new[] { "amount", "created_at", "description", "source_type", "type", "user_id" },
                values: new object[] { 200, new DateTime(2025, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Task completion bonus", "task", "EARN", new Guid("a1b2c3d4-0002-0002-0002-000000000002") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 7,
                columns: new[] { "amount", "created_at", "description", "source_type", "type", "user_id" },
                values: new object[] { 150, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Minigame reward", "minigame", "EARN", new Guid("a1b2c3d4-0002-0002-0002-000000000002") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 8,
                columns: new[] { "amount", "created_at", "description", "source_type", "type", "user_id" },
                values: new object[] { 50, new DateTime(2025, 2, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Purchased Sneakers", "shop_item", "SPEND", new Guid("a1b2c3d4-0002-0002-0002-000000000002") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 9,
                columns: new[] { "amount", "created_at", "description", "source_type", "user_id" },
                values: new object[] { 400, new DateTime(2025, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Streak milestone bonus", "streak", new Guid("a1b2c3d4-0002-0002-0002-000000000002") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 10,
                columns: new[] { "amount", "created_at", "description", "user_id" },
                values: new object[] { 100, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tutorial completion reward", new Guid("a1b2c3d4-0003-0003-0003-000000000003") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 11,
                columns: new[] { "amount", "created_at", "description", "source_type", "type", "user_id" },
                values: new object[] { 100, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Welcome bonus", "achievement", "BONUS", new Guid("a1b2c3d4-0003-0003-0003-000000000003") });

            migrationBuilder.UpdateData(
                table: "point_transactions",
                keyColumn: "transaction_id",
                keyValue: 12,
                columns: new[] { "created_at", "description", "source_type", "type", "user_id" },
                values: new object[] { new DateTime(2025, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Purchased Basic Tee", "shop_item", "SPEND", new Guid("a1b2c3d4-0003-0003-0003-000000000003") });

            migrationBuilder.UpdateData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 1,
                columns: new[] { "last_activity_date", "user_id" },
                values: new object[] { new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.UpdateData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 2,
                columns: new[] { "last_activity_date", "user_id" },
                values: new object[] { new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.UpdateData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 3,
                columns: new[] { "bonus_multiplier", "current_count", "last_activity_date", "longest_count", "user_id" },
                values: new object[] { 1.2m, 3, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, new Guid("a1b2c3d4-0002-0002-0002-000000000002") });

            migrationBuilder.UpdateData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 4,
                columns: new[] { "bonus_multiplier", "current_count", "last_activity_date", "longest_count", "user_id" },
                values: new object[] { 1.0m, 1, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, new Guid("a1b2c3d4-0003-0003-0003-000000000003") });

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000001"),
                columns: new[] { "completed_at", "created_at", "description", "user_id" },
                values: new object[] { new DateTime(2025, 3, 1, 12, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Finish the daily trivia quiz.", new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000002"),
                columns: new[] { "created_at", "description", "user_id" },
                values: new object[] { new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Win any 3 minigames this week.", new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000001"),
                columns: new[] { "category", "completed_at", "created_at", "description", "point_value", "priority", "status", "title", "user_id" },
                values: new object[] { "Shopping", new DateTime(2025, 2, 11, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 2, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Purchase any item from the shop.", 30, "LOW", "completed", "Buy first avatar item", new Guid("a1b2c3d4-0002-0002-0002-000000000002") });

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000002"),
                columns: new[] { "category", "completed_at", "created_at", "description", "due_date", "status", "title", "user_id" },
                values: new object[] { "Gaming", null, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Join a cooperative minigame session.", new DateTime(2025, 3, 15, 0, 0, 0, 0, DateTimeKind.Utc), "pending", "Play a coop game", new Guid("a1b2c3d4-0002-0002-0002-000000000002") });

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000003-0000-0000-0000-000000000001"),
                columns: new[] { "category", "completed_at", "created_at", "description", "point_value", "priority", "status", "title", "user_id" },
                values: new object[] { "Onboarding", new DateTime(2025, 2, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Finish the onboarding tutorial.", 50, "CRITICAL", "completed", "Complete tutorial", new Guid("a1b2c3d4-0003-0003-0003-000000000003") });

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
                table: "tasks",
                columns: new[] { "task_id", "category", "completed_at", "created_at", "description", "due_date", "is_recurring", "point_value", "priority", "recurrence_rule", "status", "title", "user_id" },
                values: new object[,]
                {
                    { new Guid("b0000001-0000-0000-0000-000000000003"), "Engagement", null, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Log in 7 days in a row.", new DateTime(2025, 3, 8, 0, 0, 0, 0, DateTimeKind.Utc), false, 150, "HIGH", null, "pending", "Maintain 7-day streak", new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000003-0000-0000-0000-000000000002"), "Gaming", null, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Try out any minigame.", null, false, 25, "HIGH", null, "pending", "Play first minigame", new Guid("a1b2c3d4-0003-0003-0003-000000000003") }
                });
        }
    }
}
