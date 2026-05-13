using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarItemRenderHints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "covers_hair_back",
                table: "avatar_items",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "covers_hair_front",
                table: "avatar_items",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "offset_x",
                table: "avatar_items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "offset_y",
                table: "avatar_items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "render_scale",
                table: "avatar_items",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "source_height",
                table: "avatar_items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "source_width",
                table: "avatar_items",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 1,
                columns: new[] { "covers_hair_back", "covers_hair_front", "offset_x", "offset_y", "render_scale", "source_height", "source_width" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 2,
                columns: new[] { "covers_hair_back", "covers_hair_front", "offset_x", "offset_y", "render_scale", "source_height", "source_width" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 3,
                columns: new[] { "covers_hair_back", "covers_hair_front", "offset_x", "offset_y", "render_scale", "source_height", "source_width" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 4,
                columns: new[] { "covers_hair_back", "covers_hair_front", "offset_x", "offset_y", "render_scale", "source_height", "source_width" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 5,
                columns: new[] { "covers_hair_back", "covers_hair_front", "offset_x", "offset_y", "render_scale", "source_height", "source_width" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 6,
                columns: new[] { "covers_hair_back", "covers_hair_front", "offset_x", "offset_y", "render_scale", "source_height", "source_width" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 7,
                columns: new[] { "covers_hair_back", "covers_hair_front", "offset_x", "offset_y", "render_scale", "source_height", "source_width" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 8,
                columns: new[] { "covers_hair_back", "covers_hair_front", "offset_x", "offset_y", "render_scale", "source_height", "source_width" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 9,
                columns: new[] { "covers_hair_back", "covers_hair_front", "offset_x", "offset_y", "render_scale", "source_height", "source_width" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 10,
                columns: new[] { "covers_hair_back", "covers_hair_front", "offset_x", "offset_y", "render_scale", "source_height", "source_width" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 11,
                columns: new[] { "covers_hair_back", "covers_hair_front", "offset_x", "offset_y", "render_scale", "source_height", "source_width" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 12,
                columns: new[] { "covers_hair_back", "covers_hair_front", "offset_x", "offset_y", "render_scale", "source_height", "source_width" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 13,
                columns: new[] { "covers_hair_back", "covers_hair_front", "offset_x", "offset_y", "render_scale", "source_height", "source_width" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 14,
                columns: new[] { "covers_hair_back", "covers_hair_front", "offset_x", "offset_y", "render_scale", "source_height", "source_width" },
                values: new object[] { null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 15,
                columns: new[] { "covers_hair_back", "covers_hair_front", "offset_x", "offset_y", "render_scale", "source_height", "source_width" },
                values: new object[] { null, null, null, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "covers_hair_back",
                table: "avatar_items");

            migrationBuilder.DropColumn(
                name: "covers_hair_front",
                table: "avatar_items");

            migrationBuilder.DropColumn(
                name: "offset_x",
                table: "avatar_items");

            migrationBuilder.DropColumn(
                name: "offset_y",
                table: "avatar_items");

            migrationBuilder.DropColumn(
                name: "render_scale",
                table: "avatar_items");

            migrationBuilder.DropColumn(
                name: "source_height",
                table: "avatar_items");

            migrationBuilder.DropColumn(
                name: "source_width",
                table: "avatar_items");
        }
    }
}
