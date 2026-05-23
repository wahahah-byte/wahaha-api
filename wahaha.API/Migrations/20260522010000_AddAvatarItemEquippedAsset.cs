using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <summary>
    /// Adds the nullable equipped_asset_url column to avatar_items. Used by the
    /// chibi composite to render a "worn" view that differs from the shop/catalog
    /// preview (e.g. shields: catalog shows the front face, the chibi shows the
    /// back/strap because the off-hand faces away from the camera). Null = the
    /// composite falls back to preview_asset_url.
    /// </summary>
    public partial class AddAvatarItemEquippedAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "equipped_asset_url",
                table: "avatar_items",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "equipped_asset_url",
                table: "avatar_items");
        }
    }
}
