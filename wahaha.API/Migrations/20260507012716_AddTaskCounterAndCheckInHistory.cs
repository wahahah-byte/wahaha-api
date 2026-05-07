using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskCounterAndCheckInHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "has_counter",
                table: "tasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "task_check_in_cycles",
                columns: table => new
                {
                    cycle_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    task_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    check_in_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    counter_value = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_check_in_cycles", x => x.cycle_id);
                    table.ForeignKey(
                        name: "FK_task_check_in_cycles_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000001"),
                column: "has_counter",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000002"),
                column: "has_counter",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000001"),
                column: "has_counter",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000002"),
                column: "has_counter",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000003-0000-0000-0000-000000000001"),
                column: "has_counter",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000004-0000-0000-0000-000000000001"),
                column: "has_counter",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000005-0000-0000-0000-000000000001"),
                column: "has_counter",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000006-0000-0000-0000-000000000001"),
                column: "has_counter",
                value: false);

            migrationBuilder.CreateIndex(
                name: "IX_task_check_in_cycles_task_id_check_in_date",
                table: "task_check_in_cycles",
                columns: new[] { "task_id", "check_in_date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "task_check_in_cycles");

            migrationBuilder.DropColumn(
                name: "has_counter",
                table: "tasks");
        }
    }
}
