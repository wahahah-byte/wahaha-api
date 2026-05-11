using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskCapLogAtGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "cap_log_at_goal",
                table: "tasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000001"),
                column: "cap_log_at_goal",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000002"),
                column: "cap_log_at_goal",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000001"),
                column: "cap_log_at_goal",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000002"),
                column: "cap_log_at_goal",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000003-0000-0000-0000-000000000001"),
                column: "cap_log_at_goal",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000004-0000-0000-0000-000000000001"),
                column: "cap_log_at_goal",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000005-0000-0000-0000-000000000001"),
                column: "cap_log_at_goal",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000006-0000-0000-0000-000000000001"),
                column: "cap_log_at_goal",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cap_log_at_goal",
                table: "tasks");
        }
    }
}
