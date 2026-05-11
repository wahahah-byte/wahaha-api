using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarItemGridSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // RE-style inventory footprint per item. Nullable so existing items
            // fall back to the client-side default of 1x1. The avatar page reads
            // these to size each item in the inventory grid (GridCols × GridRows).
            migrationBuilder.AddColumn<int>(
                name: "grid_cols",
                table: "avatar_items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "grid_rows",
                table: "avatar_items",
                type: "int",
                nullable: true);

            // Body items occupy a 1-wide × 2-tall slot (standing torso).
            foreach (int bodyId in new[] { 7, 8, 9, 10, 18 })
            {
                migrationBuilder.UpdateData(
                    table: "avatar_items",
                    keyColumn: "item_id",
                    keyValue: bodyId,
                    columns: new[] { "grid_cols", "grid_rows" },
                    values: new object[] { 1, 2 });
            }

            // Back-slot items (backpacks, wings) lay across two horizontal cells.
            foreach (int backId in new[] { 13, 14 })
            {
                migrationBuilder.UpdateData(
                    table: "avatar_items",
                    keyColumn: "item_id",
                    keyValue: backId,
                    columns: new[] { "grid_cols", "grid_rows" },
                    values: new object[] { 2, 1 });
            }

            // Cyber Polearm — horizontal weapon, occupies 2 cols × 1 row.
            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 20,
                columns: new[] { "grid_cols", "grid_rows" },
                values: new object[] { 2, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "grid_cols", table: "avatar_items");
            migrationBuilder.DropColumn(name: "grid_rows", table: "avatar_items");
        }
    }
}
