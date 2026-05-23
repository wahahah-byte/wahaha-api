namespace wahaha.API.Models.Domain;

// Per-slot category whitelist. Mirrors packages/shared/src/avatar/categories.ts —
// keep the two lists in sync. The admin form's dropdown only surfaces canonical
// categories; this whitelist (canonical + legacy) is what server validation accepts.
//
// The split exists so admins can edit existing rows whose category isn't canonical
// (those go in the LegacyExtras dictionary) without the request 400'ing, while still
// blocking new items from inventing freeform categories.
public static class AvatarCategoryWhitelist
{
    // Canonical names per slot, in the same order as the TypeScript dropdown.
    private static readonly IReadOnlyDictionary<ItemSlot, string[]> Canonical = new Dictionary<ItemSlot, string[]>
    {
        // Granular slots
        [ItemSlot.HAT]          = new[] { "hat", "helmet", "crown", "cap", "mask", "headband" },
        [ItemSlot.HAIR_FRONT]   = new[] { "hair", "bangs" },
        [ItemSlot.HAIR_BACK]    = new[] { "hair" },
        [ItemSlot.FACE]         = new[] { "mask", "scar", "facial-hair", "mouth" },
        [ItemSlot.EYE]          = new[] { "glasses", "eyepatch", "eye-makeup" },
        [ItemSlot.EAR]          = new[] { "earring", "ear-cuff", "ear-stud" },
        [ItemSlot.TOP]          = new[] { "t-shirt", "sweater", "hoodie", "jacket", "vest", "shirt" },
        [ItemSlot.BOTTOM]       = new[] { "pants", "shorts", "skirt", "jeans" },
        [ItemSlot.OVERALL]      = new[] { "dress", "robe", "jumpsuit", "kimono", "outfit" },
        [ItemSlot.CAPE]         = new[] { "cape", "cloak", "wings", "scarf" },
        [ItemSlot.GLOVES]       = new[] { "gloves", "mittens", "gauntlets" },
        [ItemSlot.SHOES]        = new[] { "boots", "sneakers", "sandals", "heels", "flats" },
        // WEAPON_FRONT and WEAPON_BACK accept the same categories — the slots differ only by
        // chibi render z-order. A sheathed sword and a held sword are both "sword".
        [ItemSlot.WEAPON_FRONT] = new[] { "sword", "greatsword", "dagger", "staff", "polearm", "bow", "wand", "axe", "quiver", "holster", "sheath", "backpack" },
        [ItemSlot.WEAPON_BACK]  = new[] { "sword", "greatsword", "dagger", "staff", "polearm", "bow", "wand", "axe", "quiver", "holster", "sheath", "backpack" },
        [ItemSlot.OFFHAND]      = new[] { "shield", "dagger", "orb", "tome", "lantern", "buckler" },
        [ItemSlot.WRIST]        = new[] { "bracelet", "watch", "bangle" },
        // Legacy slots (deprecated for new uploads)
        [ItemSlot.HEAD]         = new[] { "hat", "helmet", "crown", "cap", "mask", "headband", "headwear" },
        [ItemSlot.HAIR]         = new[] { "hair" },
        [ItemSlot.BODY]         = new[] { "outerwear", "outfit", "top", "bottom" },
        [ItemSlot.HAND]         = new[] { "sword", "dagger", "axe", "wand", "tool", "weapon", "accessory" },
        [ItemSlot.BACK]         = new[] { "cape", "cloak", "wings" },
        [ItemSlot.FEET]         = new[] { "footwear", "boots", "sneakers" },
    };

    // Accepted-but-hidden categories for backward compat with rows seeded before this list.
    // The admin dropdown does NOT surface these — they round-trip on edit but new items can't pick them.
    private static readonly IReadOnlyDictionary<ItemSlot, string[]> LegacyExtras = new Dictionary<ItemSlot, string[]>
    {
        [ItemSlot.HAT]          = new[] { "headwear" },
        [ItemSlot.HEAD]         = new[] { "headwear" },
        [ItemSlot.FACE]         = new[] { "accessory" },
        [ItemSlot.HAND]         = new[] { "accessory", "weapon", "weapon_staff" },
        [ItemSlot.WEAPON_FRONT] = new[] { "weapon", "weapon_staff" },
        [ItemSlot.CAPE]         = new[] { "cape" },
        [ItemSlot.BODY]         = new[] { "outerwear", "outfit" },
    };

    public static bool IsValid(ItemSlot slot, string? category)
    {
        if (string.IsNullOrWhiteSpace(category)) return false;
        var normalised = category.Trim().ToLowerInvariant();
        if (Canonical.TryGetValue(slot, out var canon)
            && canon.Any(c => string.Equals(c, normalised, StringComparison.OrdinalIgnoreCase)))
            return true;
        return LegacyExtras.TryGetValue(slot, out var legacy)
            && legacy.Any(c => string.Equals(c, normalised, StringComparison.OrdinalIgnoreCase));
    }

    // Joined list for error messages: "canonical: a, b, c (legacy also accepted: x, y)".
    public static string AllowedFor(ItemSlot slot)
    {
        var canon = Canonical.TryGetValue(slot, out var c) ? string.Join(", ", c) : "(none)";
        var legacy = LegacyExtras.TryGetValue(slot, out var l) && l.Length > 0
            ? $" (legacy also accepted: {string.Join(", ", l)})"
            : "";
        return $"canonical: {canon}{legacy}";
    }
}
