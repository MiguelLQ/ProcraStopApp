namespace ProcraStop.Application.Habits.Analytics;

public sealed class HabitAnalyticsDto
{
    public int ActiveHabits { get; set; }
    public int ScheduledToday { get; set; }
    public int CompletedToday { get; set; }
    public int ExpectedThisWeek { get; set; }
    public int CompletedThisWeek { get; set; }
    public int AtRiskHabits { get; set; }
    public int BinaryHabits { get; set; }
    public int QuantitativeHabits { get; set; }
    public int CurrentStreak { get; set; }
    public int BestStreak { get; set; }
    public double ConsistencyRate { get; set; }
    public double FailureRate { get; set; }
    public string[] WeeklyLabels { get; set; } = Array.Empty<string>();
    public double[] WeeklyCompleted { get; set; } = Array.Empty<double>();
    public string[] StatusLabels { get; set; } = Array.Empty<string>();
    public double[] StatusValues { get; set; } = Array.Empty<double>();
    public string[] TypeLabels { get; set; } = Array.Empty<string>();
    public double[] TypeValues { get; set; } = Array.Empty<double>();
    public string[] RiskLabels { get; set; } = Array.Empty<string>();
    public double[] RiskValues { get; set; } = Array.Empty<double>();
}
