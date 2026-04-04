using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace wahaha.API.Migrations.AuthDb
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "auth",
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "6c181267-198e-4f13-82ce-f98dae3b292b", "6c181267-198e-4f13-82ce-f98dae3b292b", "Admin", "ADMIN" },
                    { "b63a2149-ad02-408d-bea7-8fe32157286f", "b63a2149-ad02-408d-bea7-8fe32157286f", "Moderator", "MODERATOR" },
                    { "dbe856d6-3740-4b4f-b3c2-b694e102a738", "dbe856d6-3740-4b4f-b3c2-b694e102a738", "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "auth",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6c181267-198e-4f13-82ce-f98dae3b292b");

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b63a2149-ad02-408d-bea7-8fe32157286f");

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "dbe856d6-3740-4b4f-b3c2-b694e102a738");
        }
    }
}
