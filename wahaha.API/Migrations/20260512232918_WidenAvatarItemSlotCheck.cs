using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <summary>
    /// Widens the CK_avatar_items_slot CHECK constraint to accept the new
    /// MapleStory-style granular slot names introduced when the C# ItemSlot
    /// enum was extended. The original constraint (added in
    /// MoveHairToHairSlot) only allowed the seven legacy values, so any
    /// INSERT with a new value (e.g. "HAT", "OVERALL", "WEAPON_FRONT")
    /// fails with SqlException CK_avatar_items_slot.
    ///
    /// The slot column is stored as a string (NVARCHAR), not as the int
    /// underlying the enum — EF was configured that way at column creation.
    /// That's why this is a string-set CHECK rather than an int-range one.
    /// </summary>
    public partial class WidenAvatarItemSlotCheck : Migration
    {
        // Single source of truth for the allowed slot names. Keep in sync
        // with the C# ItemSlot enum in Models/Domain/AvatarItems.cs.
        private const string AllowedSlots =
            "'HEAD','HAIR','BODY','HAND','FACE','BACK','FEET'," +
            "'WEAPON_BACK','CAPE','HAIR_BACK','BOTTOM','TOP','OVERALL'," +
            "'GLOVES','SHOES','HAIR_FRONT','EYE','EAR','HAT'," +
            "'WEAPON_FRONT','WRIST'";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop any existing slot constraint by name (matches both the
            // hash-suffixed auto-generated one from the original migration
            // and the explicitly named CK_avatar_items_slot that
            // MoveHairToHairSlot rebuilt).
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
            // Reverting only restores the legacy-7 whitelist. Any rows that
            // used a granular slot after Up ran would now violate the
            // constraint — caller must hand-migrate those rows back to
            // legacy slot names before applying Down.
            migrationBuilder.Sql(@"
                IF OBJECT_ID('CK_avatar_items_slot', 'C') IS NOT NULL
                    ALTER TABLE dbo.avatar_items DROP CONSTRAINT CK_avatar_items_slot;
            ");
            migrationBuilder.Sql(@"
                ALTER TABLE dbo.avatar_items ADD CONSTRAINT CK_avatar_items_slot
                CHECK (slot IN ('HEAD','HAIR','BODY','HAND','FACE','BACK','FEET'));
            ");
        }
    }
}
