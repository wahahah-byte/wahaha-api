using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedPngAvatarItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seeds the four PNG-backed items that used to live in the frontend
            // mockAvatar catalogue. Mirrors the URLs already in blob storage.
            // Hair and Helmet both occupy HEAD — EquipAsync's slot uniqueness
            // means a user can wear one or the other at a time.
            migrationBuilder.InsertData(
                table: "avatar_items",
                columns: new[] { "item_id", "category", "cost", "description", "is_available", "name", "preview_asset_url", "rarity", "slot" },
                values: new object[,]
                {
                    { 17, "headwear",  75,  "A futuristic alien helmet.",                  true, "Alien Neo Helmet",  "https://wahaha.blob.core.windows.net/avatar-items/hat_alien_neo.png",                 "UNCOMMON", "HEAD" },
                    { 18, "outerwear", 40,  "A cozy knit sweater.",                        true, "Sweater",           "https://wahaha.blob.core.windows.net/avatar-items/sweater_knit_white.png",            "COMMON",   "BODY" },
                    { 19, "hair",      60,  "Long wavy brown hair.",                       true, "Seraph Wave Brown", "https://wahaha.blob.core.windows.net/avatar-items/hair_seraph_wave_brown.png",        "UNCOMMON", "HAIR" },
                    { 20, "weapon",    500, "An alien cyber polearm crackling with energy.", true, "Cyber Polearm",   "https://wahaha.blob.core.windows.net/avatar-items/weapon_polearm_alien_cyber.png",    "EPIC",     "HAND" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "avatar_items", keyColumn: "item_id", keyValue: 17);
            migrationBuilder.DeleteData(table: "avatar_items", keyColumn: "item_id", keyValue: 18);
            migrationBuilder.DeleteData(table: "avatar_items", keyColumn: "item_id", keyValue: 19);
            migrationBuilder.DeleteData(table: "avatar_items", keyColumn: "item_id", keyValue: 20);
        }
    }
}
