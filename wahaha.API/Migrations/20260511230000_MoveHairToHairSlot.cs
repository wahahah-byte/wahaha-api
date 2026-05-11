using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <inheritdoc />
    public partial class MoveHairToHairSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL Server auto-named a CHECK constraint on avatar_items.slot
            // when the column was first created (CK__avatar_ite__slot__<hash>),
            // restricting it to the original six enum values. Updating the row
            // to 'HAIR' would violate that constraint, so we drop it
            // dynamically (the hash suffix differs per database), run the
            // UPDATE, then re-add a named constraint that includes HAIR.
            migrationBuilder.Sql(@"
                DECLARE @cn NVARCHAR(200);
                SELECT @cn = name FROM sys.check_constraints
                    WHERE parent_object_id = OBJECT_ID('dbo.avatar_items')
                      AND name LIKE '%slot%';
                IF @cn IS NOT NULL
                    EXEC('ALTER TABLE dbo.avatar_items DROP CONSTRAINT [' + @cn + ']');
            ");

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 19,
                column: "slot",
                value: "HAIR");

            migrationBuilder.Sql(@"
                ALTER TABLE dbo.avatar_items ADD CONSTRAINT CK_avatar_items_slot
                CHECK (slot IN ('HEAD', 'HAIR', 'BODY', 'HAND', 'FACE', 'BACK', 'FEET'));
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID('CK_avatar_items_slot', 'C') IS NOT NULL
                    ALTER TABLE dbo.avatar_items DROP CONSTRAINT CK_avatar_items_slot;
            ");

            migrationBuilder.UpdateData(
                table: "avatar_items",
                keyColumn: "item_id",
                keyValue: 19,
                column: "slot",
                value: "HEAD");

            migrationBuilder.Sql(@"
                ALTER TABLE dbo.avatar_items ADD CONSTRAINT CK_avatar_items_slot
                CHECK (slot IN ('HEAD', 'BODY', 'HAND', 'FACE', 'BACK', 'FEET'));
            ");
        }
    }
}
