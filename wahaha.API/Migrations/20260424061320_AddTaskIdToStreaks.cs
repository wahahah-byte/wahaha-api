using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskIdToStreaks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "task_id",
                table: "streaks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 1,
                column: "task_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 2,
                column: "task_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 3,
                column: "task_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 4,
                column: "task_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 5,
                column: "task_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 6,
                column: "task_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "streaks",
                keyColumn: "streak_id",
                keyValue: 7,
                column: "task_id",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_streaks_task_id",
                table: "streaks",
                column: "task_id");

            migrationBuilder.AddForeignKey(
                name: "FK_streaks_tasks_task_id",
                table: "streaks",
                column: "task_id",
                principalTable: "tasks",
                principalColumn: "task_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_streaks_tasks_task_id",
                table: "streaks");

            migrationBuilder.DropIndex(
                name: "IX_streaks_task_id",
                table: "streaks");

            migrationBuilder.DropColumn(
                name: "task_id",
                table: "streaks");
        }
    }
}
