namespace wahaha.API.Models;

public static class PointCaps
{
    public const int RegularDaily = 200;
    public const int RecurringDaily = 200;
    public const int PerCategoryRegularDaily = 50;
    public const int PerCategoryRecurringDaily = 50;

    public static readonly IReadOnlyDictionary<string, int> PerTaskValue =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Career"]       = 25,
            ["Dev"]          = 25,
            ["Design"]       = 25,
            ["Learning"]     = 25,
            ["Study"]        = 25,
            ["Work"]         = 25,
            ["Finance"]      = 20,
            ["Health"]       = 20,
            ["Productivity"] = 20,
            ["Fitness"]      = 15,
            ["Personal"]     = 15,
            ["Other"]        = 15,
            ["Habits"]       = 10,
        };

    public static int MaxFor(string? category)
    {
        if (string.IsNullOrWhiteSpace(category)) return 25;
        return PerTaskValue.TryGetValue(category, out var v) ? v : 25;
    }
}
