using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class WahahaDbCntext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "submitted",
                table: "tasks",
                type: "bit",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000001"),
                column: "submitted",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000002"),
                column: "submitted",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000001"),
                column: "submitted",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000002"),
                column: "submitted",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000003-0000-0000-0000-000000000001"),
                column: "submitted",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000004-0000-0000-0000-000000000001"),
                column: "submitted",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000005-0000-0000-0000-000000000001"),
                column: "submitted",
                value: false);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: new Guid("b0000006-0000-0000-0000-000000000001"),
                column: "submitted",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "submitted",
                table: "tasks");
        }
    }
}
