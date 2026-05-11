using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class GrantPngItemsToPixelQueen : Migration
    {
        // PixelQueen — the most-equipped seeded user in UpdateUserEmails.
        private static readonly Guid PixelQueenId = new("8e43c4d2-8381-44c7-808c-8f4c98d8d57b");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Grants the four PNG-backed items (Alien Neo Helmet, Sweater,
            // Seraph Wave Brown, Cyber Polearm — item_id 17..20 from
            // SeedPngAvatarItems) to PixelQueen. All unequipped so she can
            // pick them up from the inventory grid.
            migrationBuilder.InsertData(
                table: "user_inventory",
                columns: new[] { "inventory_id", "acquired_at", "is_equipped", "item_id", "user_id" },
                values: new object[,]
                {
                    { 21, new DateTime(2026, 5, 11, 0, 0, 0, 0, DateTimeKind.Utc), false, 17, PixelQueenId },
                    { 22, new DateTime(2026, 5, 11, 0, 0, 0, 0, DateTimeKind.Utc), false, 18, PixelQueenId },
                    { 23, new DateTime(2026, 5, 11, 0, 0, 0, 0, DateTimeKind.Utc), false, 19, PixelQueenId },
                    { 24, new DateTime(2026, 5, 11, 0, 0, 0, 0, DateTimeKind.Utc), false, 20, PixelQueenId },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "user_inventory", keyColumn: "inventory_id", keyValue: 21);
            migrationBuilder.DeleteData(table: "user_inventory", keyColumn: "inventory_id", keyValue: 22);
            migrationBuilder.DeleteData(table: "user_inventory", keyColumn: "inventory_id", keyValue: 23);
            migrationBuilder.DeleteData(table: "user_inventory", keyColumn: "inventory_id", keyValue: 24);
        }
    }
}
