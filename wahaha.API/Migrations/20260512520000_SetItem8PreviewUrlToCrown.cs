using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class SetItem8PreviewUrlToCrown : Migration
    {
        private const string CrownUrl = "https://wahaha.blob.core.windows.net/avatar-items/hat_crown_flower.png";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 8,
                column: "preview_asset_url",
                value: CrownUrl);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Original seed value for item 8 was NULL.
            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 8,
                column: "preview_asset_url",
                value: null);
        }
    }
}
