namespace wahaha.API.Handlers.Tasks;

internal static class StreakBonusMultiplier
{
    public static decimal Compute(int count) => count switch
    {
        >= 30 => 2.0m,
        >= 14 => 1.8m,
        >= 7  => 1.5m,
        >= 3  => 1.2m,
        _     => 1.0m,
    };
}
