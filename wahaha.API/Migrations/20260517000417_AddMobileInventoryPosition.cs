using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMobileInventoryPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "position_x_mobile",
                table: "user_inventory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "position_y_mobile",
                table: "user_inventory",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 1,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 2,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 3,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 4,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 5,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 6,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 7,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 8,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 9,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 10,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 11,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 12,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 13,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 14,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 15,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 16,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 17,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 18,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 19,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "user_inventory",
                keyColumn: "inventory_id",
                keyValue: 20,
                columns: new[] { "position_x_mobile", "position_y_mobile" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "position_x_mobile",
                table: "user_inventory");

            migrationBuilder.DropColumn(
                name: "position_y_mobile",
                table: "user_inventory");
        }
    }
}
