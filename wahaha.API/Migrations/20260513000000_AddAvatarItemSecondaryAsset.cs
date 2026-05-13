using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <summary>
    /// Adds the nullable secondary_asset_url column to avatar_items so a single
    /// row can drive two render layers (Option B for the hair front/back
    /// problem). The primary PreviewAssetUrl drives the inventory card and the
    /// item's "own" slot z-order; the secondary, when present, draws as an
    /// extra HAIR_BACK layer in the chibi composite. Other slots may use this
    /// later but the only current consumer is HAIR_FRONT.
    /// </summary>
    public partial class AddAvatarItemSecondaryAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "secondary_asset_url",
                table: "avatar_items",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "secondary_asset_url",
                table: "avatar_items");
        }
    }
}
