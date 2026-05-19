namespace wahaha.API.Handlers.Tasks;

internal static class TaskCycleHelpers
{
    public static DateTime GetCycleStart(DateTime dueDate, string? rule)
    {
        var d = dueDate.Date;
        return rule switch
        {
            "daily"    => d.AddDays(-1),
            "weekdays" => d.AddDays(-1),
            "weekly"   => d.AddDays(-7),
            "biweekly" => d.AddDays(-14),
            "monthly"  => d.AddMonths(-1),
            _          => d.AddDays(-1),
        };
    }

    public static DateTime? ComputeNextDueDate(DateTime? dueDate, string? rule, DateTime clientToday)
    {
        var baseDate = dueDate?.Date ?? clientToday;
        if (baseDate < clientToday) baseDate = clientToday;
        var base_ = new DateTime(baseDate.Year, baseDate.Month, baseDate.Day, 12, 0, 0, DateTimeKind.Utc);
        switch (rule)
        {
            case "daily": base_ = base_.AddDays(1); break;
            case "weekdays":
                base_ = base_.AddDays(1);
                while (base_.DayOfWeek == DayOfWeek.Saturday || base_.DayOfWeek == DayOfWeek.Sunday)
                    base_ = base_.AddDays(1);
                break;
            case "weekly": base_ = base_.AddDays(7); break;
            case "biweekly": base_ = base_.AddDays(14); break;
            case "monthly": base_ = base_.AddMonths(1); break;
        }
        return base_;
    }
}
