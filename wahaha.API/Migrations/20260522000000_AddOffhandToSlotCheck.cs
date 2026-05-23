using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <summary>
    /// Extends CK_avatar_items_slot to allow the new OFFHAND slot
    /// (shields, daggers, orbs, tomes, etc.). Mirrors the previous
    /// WidenAvatarItemSlotCheck pattern; adds OFFHAND to the
    /// AllowedSlots whitelist.
    /// </summary>
    public partial class AddOffhandToSlotCheck : Migration
    {
        // Keep in sync with the C# ItemSlot enum in
        // Models/Domain/AvatarItems.cs.
        private const string AllowedSlots =
            "'HEAD','HAIR','BODY','HAND','FACE','BACK','FEET'," +
            "'WEAPON_BACK','CAPE','HAIR_BACK','BOTTOM','TOP','OVERALL'," +
            "'GLOVES','SHOES','HAIR_FRONT','EYE','EAR','HAT'," +
            "'WEAPON_FRONT','WRIST','OFFHAND'";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @cn NVARCHAR(200);
                SELECT @cn = name FROM sys.check_constraints
                    WHERE parent_object_id = OBJECT_ID('dbo.avatar_items')
                      AND name LIKE '%slot%';
                IF @cn IS NOT NULL
                    EXEC('ALTER TABLE dbo.avatar_items DROP CONSTRAINT [' + @cn + ']');
            ");

            migrationBuilder.Sql($@"
                ALTER TABLE dbo.avatar_items ADD CONSTRAINT CK_avatar_items_slot
                CHECK (slot IN ({AllowedSlots}));
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to the prior (pre-OFFHAND) whitelist. Any rows that used
            // OFFHAND after Up ran would violate the constraint — caller must
            // hand-migrate those rows back to another slot before Down.
            const string PriorSlots =
                "'HEAD','HAIR','BODY','HAND','FACE','BACK','FEET'," +
                "'WEAPON_BACK','CAPE','HAIR_BACK','BOTTOM','TOP','OVERALL'," +
                "'GLOVES','SHOES','HAIR_FRONT','EYE','EAR','HAT'," +
                "'WEAPON_FRONT','WRIST'";

            migrationBuilder.Sql(@"
                IF OBJECT_ID('CK_avatar_items_slot', 'C') IS NOT NULL
                    ALTER TABLE dbo.avatar_items DROP CONSTRAINT CK_avatar_items_slot;
            ");
            migrationBuilder.Sql($@"
                ALTER TABLE dbo.avatar_items ADD CONSTRAINT CK_avatar_items_slot
                CHECK (slot IN ({PriorSlots}));
            ");
        }
    }
}
