using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckInCycleTypeAndUndo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cycle_type",
                table: "task_check_in_cycles",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "checkin");

            migrationBuilder.AddColumn<int>(
                name: "points_awarded",
                table: "task_check_in_cycles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "point_transaction_id",
                table: "task_check_in_cycles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "previous_due_date",
                table: "task_check_in_cycles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "previous_last_check_in_date",
                table: "task_check_in_cycles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "previous_streak_count",
                table: "task_check_in_cycles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "previous_longest_count",
                table: "task_check_in_cycles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "previous_streak_last_activity",
                table: "task_check_in_cycles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "previous_streak_is_active",
                table: "task_check_in_cycles",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "previous_streak_bonus_multiplier",
                table: "task_check_in_cycles",
                type: "decimal(18,4)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "previous_streak_bonus_multiplier",
                table: "task_check_in_cycles");

            migrationBuilder.DropColumn(
                name: "previous_streak_is_active",
                table: "task_check_in_cycles");

            migrationBuilder.DropColumn(
                name: "previous_streak_last_activity",
                table: "task_check_in_cycles");

            migrationBuilder.DropColumn(
                name: "previous_longest_count",
                table: "task_check_in_cycles");

            migrationBuilder.DropColumn(
                name: "previous_streak_count",
                table: "task_check_in_cycles");

            migrationBuilder.DropColumn(
                name: "previous_last_check_in_date",
                table: "task_check_in_cycles");

            migrationBuilder.DropColumn(
                name: "previous_due_date",
                table: "task_check_in_cycles");

            migrationBuilder.DropColumn(
                name: "point_transaction_id",
                table: "task_check_in_cycles");

            migrationBuilder.DropColumn(
                name: "points_awarded",
                table: "task_check_in_cycles");

            migrationBuilder.DropColumn(
                name: "cycle_type",
                table: "task_check_in_cycles");
        }
    }
}
