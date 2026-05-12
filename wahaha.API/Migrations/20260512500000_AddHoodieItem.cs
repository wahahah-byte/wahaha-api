using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class AddHoodieItem : Migration
    {
        // PixelQueen — currently-logged-in user. Same id as the previous
        // GrantPngItemsToPixelQueen migration.
        private const string PixelQueenId = "8e43c4d2-8381-44c7-808c-8f4c98d8d57b";
        private const string PreviewAssetUrl = "https://wahaha.blob.core.windows.net/avatar-items/hoodie_casual_grey.png";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hardcoded item_ids collide with rows added via the admin API
            // after the seed migrations ran, so insert without an explicit
            // primary key and round-trip the auto-assigned id via
            // SCOPE_IDENTITY() into the user_inventory row.
            migrationBuilder.Sql($@"
DECLARE @NewItemId INT;

INSERT INTO avatar_items
    (category, cost, description, grid_cols, grid_rows, is_available, name, preview_asset_url, rarity, slot)
VALUES
    ('outerwear', 50, 'A casual grey hoodie.', 1, 1, 1, 'Casual Hoodie',
     '{PreviewAssetUrl}', 'COMMON', 'BODY');

SET @NewItemId = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO user_inventory
    (acquired_at, is_equipped, item_id, user_id)
VALUES
    (SYSUTCDATETIME(), 0, @NewItemId, '{PixelQueenId}');
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Match-by-URL because the assigned ids aren't known statically.
            migrationBuilder.Sql($@"
DELETE ui FROM user_inventory ui
INNER JOIN avatar_items ai ON ai.item_id = ui.item_id
WHERE ai.preview_asset_url = '{PreviewAssetUrl}';

DELETE FROM avatar_items WHERE preview_asset_url = '{PreviewAssetUrl}';
");
        }
    }
}
