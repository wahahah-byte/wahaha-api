using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfilePicture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "profile_picture_url",
                table: "users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("60892566-964c-4cdd-b9d1-f067981cd271"),
                column: "profile_picture_url",
                value: null);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("80b6c179-8747-4a29-b112-7d774871a435"),
                column: "profile_picture_url",
                value: null);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("8e43c4d2-8381-44c7-808c-8f4c98d8d57b"),
                column: "profile_picture_url",
                value: null);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("9e8e1aa6-273c-48c3-8634-56b630d6f3e3"),
                column: "profile_picture_url",
                value: null);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("d5b63d3e-6049-44ed-92df-69560148f8a3"),
                column: "profile_picture_url",
                value: null);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("ea80e4bc-017d-4729-99d3-29ad8a5d9b47"),
                column: "profile_picture_url",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "profile_picture_url",
                table: "users");
        }
    }
}
