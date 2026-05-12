using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class ResizeBodyItemsToOneByOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Torso items used to take a 1×2 footprint in the inventory grid;
            // we now treat them as single-cell so the inventory packs denser
            // and rotation doesn't apply to them at all.
            foreach (int bodyId in new[] { 7, 8, 9, 10, 18 })
            {
                migrationBuilder.UpdateData(
                    table: "avatar_items",
                    keyColumn: "item_id",
                    keyValue: bodyId,
                    columns: new[] { "grid_cols", "grid_rows" },
                    values: new object[] { 1, 1 });
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (int bodyId in new[] { 7, 8, 9, 10, 18 })
            {
                migrationBuilder.UpdateData(
                    table: "avatar_items",
                    keyColumn: "item_id",
                    keyValue: bodyId,
                    columns: new[] { "grid_cols", "grid_rows" },
                    values: new object[] { 1, 2 });
            }
        }
    }
}
