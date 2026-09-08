namespace ProcraStop.Application.Habits.Ranking;

public sealed class HabitDisciplineRankingDto
{
    public int ActiveHabits { get; init; }
    public int EliteHabits { get; init; }
    public int AtRiskHabits { get; init; }
    public double AverageScore { get; init; }
    public IReadOnlyList<HabitDisciplineRankDto> Items { get; init; } = Array.Empty<HabitDisciplineRankDto>();
}
