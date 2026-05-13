using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wahaha.API.Migrations
{
    /// <summary>
    /// Backfills the new render-hint columns (added in the prior migration) for
    /// the three filenames that previously lived in the frontend RENDER_HINTS
    /// dictionary at src/app/avatar/page.tsx. Matches rows by PreviewAssetUrl
    /// ending in the filename so it works whether the blob was uploaded by hand
    /// or via the avatar-items container — and is a no-op when no row matches
    /// (the values previously came from mock-only frontend data, so the seeded
    /// rows may not include these blobs at all).
    ///
    /// Down is purposefully a no-op: rolling this back doesn't drop the columns
    /// (the prior migration handles that) and reverting the *values* to NULL
    /// is unsafe because we'd overwrite any per-item tuning an admin did since
    /// this ran.
    /// </summary>
    public partial class BackfillAvatarItemRenderHints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // hat_alien_neo.png — full-coverage helmet that hides hair; needs
            // slight scale-up and a downward nudge to align with the chibi.
            // Migrated from RENDER_HINTS: { coversHair: true, renderScale: 1.2, offsetY: 10 }
            // coversHair is split into the two granular flags here.
            migrationBuilder.Sql(@"
                UPDATE avatar_items
                SET covers_hair_front = 1,
                    covers_hair_back  = 1,
                    render_scale      = 1.2,
                    offset_y          = 10
                WHERE preview_asset_url LIKE '%hat_alien_neo.png';
            ");

            // hair_seraph_wave_brown.png — long brown hair drawn ~11 source
            // pixels left of canvas centre; nudge right to align with head.
            // Migrated from RENDER_HINTS: { offsetX: 11 }
            migrationBuilder.Sql(@"
                UPDATE avatar_items
                SET offset_x = 11
                WHERE preview_asset_url LIKE '%hair_seraph_wave_brown.png';
            ");

            // weapon_polearm_alien_cyber.png — oversized weapon on a 384x384
            // canvas (vs base 256x384); needs source dims + nudges + scale.
            // Migrated from RENDER_HINTS: { sourceWidth: 384, sourceHeight: 384, offsetX: 6, offsetY: -8, renderScale: 1.25 }
            migrationBuilder.Sql(@"
                UPDATE avatar_items
                SET source_width  = 384,
                    source_height = 384,
                    offset_x      = 6,
                    offset_y      = -8,
                    render_scale  = 1.25
                WHERE preview_asset_url LIKE '%weapon_polearm_alien_cyber.png';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty — see class summary above.
        }
    }
}
