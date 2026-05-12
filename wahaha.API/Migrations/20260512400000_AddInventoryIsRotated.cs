using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryIsRotated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Persists the dragged-item rotation (Q/E in the avatar page)
            // so the user's grid layout survives reloads.
            migrationBuilder.AddColumn<bool>(
                name: "is_rotated",
                table: "user_inventory",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "is_rotated", table: "user_inventory");
        }
    }
}
