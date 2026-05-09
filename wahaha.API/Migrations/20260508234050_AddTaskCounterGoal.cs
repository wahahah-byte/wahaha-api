using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskCounterGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "counter_goal",
                table: "tasks",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000001"),
                column: "counter_goal",
                value: null);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000002"),
                column: "counter_goal",
                value: null);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000001"),
                column: "counter_goal",
                value: null);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000002"),
                column: "counter_goal",
                value: null);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000003-0000-0000-0000-000000000001"),
                column: "counter_goal",
                value: null);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000004-0000-0000-0000-000000000001"),
                column: "counter_goal",
                value: null);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000005-0000-0000-0000-000000000001"),
                column: "counter_goal",
                value: null);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000006-0000-0000-0000-000000000001"),
                column: "counter_goal",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "counter_goal",
                table: "tasks");
        }
    }
}
