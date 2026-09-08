namespace ProcraStop.Application.Habits.Ranking;

public sealed class HabitDisciplineRankDto
{
    public int Position { get; init; }
    public Guid HabitId { get; init; }
    public string Name { get; init; } = string.Empty;
    public double Score { get; init; }
    public double ConsistencyRate { get; init; }
    public double FailureRate { get; init; }
    public double Trend { get; init; }
    public int CurrentStreak { get; init; }
    public int CompletedDays { get; init; }
    public int ExpectedDays { get; init; }
    public bool IsAtRisk { get; init; }
    public string Status { get; init; } = string.Empty;

    public HabitDisciplineRankDto WithPosition(int position)
    {
        return new HabitDisciplineRankDto
        {
            Position = position,
            HabitId = HabitId,
            Name = Name,
            Score = Score,
            ConsistencyRate = ConsistencyRate,
            FailureRate = FailureRate,
            Trend = Trend,
            CurrentStreak = CurrentStreak,
            CompletedDays = CompletedDays,
            ExpectedDays = ExpectedDays,
            IsAtRisk = IsAtRisk,
            Status = Status
        };
    }
}
