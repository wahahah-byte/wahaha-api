using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarItemContentBounds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "content_max_x",
                table: "avatar_items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "content_max_y",
                table: "avatar_items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "content_min_x",
                table: "avatar_items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "content_min_y",
                table: "avatar_items",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 1,
                columns: new[] { "content_max_x", "content_max_y", "content_min_x", "content_min_y" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 2,
                columns: new[] { "content_max_x", "content_max_y", "content_min_x", "content_min_y" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 3,
                columns: new[] { "content_max_x", "content_max_y", "content_min_x", "content_min_y" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 4,
                columns: new[] { "content_max_x", "content_max_y", "content_min_x", "content_min_y" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 5,
                columns: new[] { "content_max_x", "content_max_y", "content_min_x", "content_min_y" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 6,
                columns: new[] { "content_max_x", "content_max_y", "content_min_x", "content_min_y" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 7,
                columns: new[] { "content_max_x", "content_max_y", "content_min_x", "content_min_y" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 8,
                columns: new[] { "content_max_x", "content_max_y", "content_min_x", "content_min_y" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 9,
                columns: new[] { "content_max_x", "content_max_y", "content_min_x", "content_min_y" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 10,
                columns: new[] { "content_max_x", "content_max_y", "content_min_x", "content_min_y" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 11,
                columns: new[] { "content_max_x", "content_max_y", "content_min_x", "content_min_y" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 12,
                columns: new[] { "content_max_x", "content_max_y", "content_min_x", "content_min_y" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 13,
                columns: new[] { "content_max_x", "content_max_y", "content_min_x", "content_min_y" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 14,
                columns: new[] { "content_max_x", "content_max_y", "content_min_x", "content_min_y" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 15,
                columns: new[] { "content_max_x", "content_max_y", "content_min_x", "content_min_y" },
                values: new object[] { null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content_max_x",
                table: "avatar_items");

            migrationBuilder.DropColumn(
                name: "content_max_y",
                table: "avatar_items");

            migrationBuilder.DropColumn(
                name: "content_min_x",
                table: "avatar_items");

            migrationBuilder.DropColumn(
                name: "content_min_y",
                table: "avatar_items");
        }
    }
}
