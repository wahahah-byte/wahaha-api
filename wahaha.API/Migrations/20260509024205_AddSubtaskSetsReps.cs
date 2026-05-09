using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSubtaskSetsReps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "reps_target",
                table: "subtasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sets_completed",
                table: "subtasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sets_target",
                table: "subtasks",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reps_target",
                table: "subtasks");

            migrationBuilder.DropColumn(
                name: "sets_completed",
                table: "subtasks");

            migrationBuilder.DropColumn(
                name: "sets_target",
                table: "subtasks");
        }
    }
}
