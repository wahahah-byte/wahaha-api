using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class GrantHoodieToTaeyeon : Migration
    {
        private const string HoodieUrl = "https://wahaha.blob.core.windows.net/avatar-items/hoodie_casual_grey.png";
        private const string Email = "taeyeon+test@wahaha.com";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Look the hoodie and the user up dynamically so we don't depend
            // on the auto-assigned ids from AddHoodieItem or on the seeded
            // user list (the test user may have been created later).
            migrationBuilder.Sql($@"
DECLARE @ItemId INT = (SELECT TOP 1 item_id FROM avatar_items WHERE preview_asset_url = '{HoodieUrl}');
DECLARE @UserId UNIQUEIDENTIFIER = (SELECT TOP 1 user_id FROM users WHERE email = '{Email}');

IF @ItemId IS NULL OR @UserId IS NULL
    THROW 51000, 'AddHoodieItem must run first and user must exist.', 1;

IF NOT EXISTS (
    SELECT 1 FROM user_inventory WHERE item_id = @ItemId AND user_id = @UserId
)
BEGIN
    INSERT INTO user_inventory (acquired_at, is_equipped, item_id, user_id)
    VALUES (SYSUTCDATETIME(), 0, @ItemId, @UserId);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
DELETE ui FROM user_inventory ui
INNER JOIN avatar_items ai ON ai.item_id = ui.item_id
INNER JOIN users u ON u.user_id = ui.user_id
WHERE ai.preview_asset_url = '{HoodieUrl}'
  AND u.email = '{Email}';
");
        }
    }
}
