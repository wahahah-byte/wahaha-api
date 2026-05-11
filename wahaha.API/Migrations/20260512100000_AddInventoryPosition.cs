using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Grid position for each inventory row — used by the
            // Resident-Evil-style avatar inventory page. Nullable so existing
            // rows can be auto-placed by the client on first load; the client
            // PATCHes the assigned value back via /api/UserInventory/{id}/position.
            migrationBuilder.AddColumn<int>(
                name: "position_x",
                table: "user_inventory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "position_y",
                table: "user_inventory",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "position_x", table: "user_inventory");
            migrationBuilder.DropColumn(name: "position_y", table: "user_inventory");
        }
    }
}
