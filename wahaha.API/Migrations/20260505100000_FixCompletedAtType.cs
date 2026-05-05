using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <summary>
    /// The live tasks.completed_at column had drifted to SQL `date` (date-only, precision 0)
    /// even though every EF migration declared it as datetime2. That truncated the time on
    /// every CompleteAsync write, which surfaced as "8:00 PM previous day" for ET users
    /// when midnight UTC got rendered locally. This migration restores the intended type
    /// so future completions preserve the actual time.
    /// </summary>
    public partial class FixCompletedAtType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // chk_dates depends on completed_at; drop it before widening the column,
            // then re-add it afterward.
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'chk_dates') ALTER TABLE tasks DROP CONSTRAINT chk_dates;");

            migrationBuilder.AlterColumn<System.DateTime>(
                name: "completed_at",
                table: "tasks",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(System.DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.Sql("ALTER TABLE tasks ADD CONSTRAINT chk_dates CHECK ([completed_at] IS NULL OR [completed_at] >= [created_at]);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'chk_dates') ALTER TABLE tasks DROP CONSTRAINT chk_dates;");

            migrationBuilder.AlterColumn<System.DateTime>(
                name: "completed_at",
                table: "tasks",
                type: "date",
                nullable: true,
                oldClrType: typeof(System.DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.Sql("ALTER TABLE tasks ADD CONSTRAINT chk_dates CHECK ([completed_at] IS NULL OR [completed_at] >= [created_at]);");
        }
    }
}
