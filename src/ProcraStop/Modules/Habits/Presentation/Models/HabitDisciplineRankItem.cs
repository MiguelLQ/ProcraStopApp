using ProcraStop.Application.Habits.Ranking;

namespace ProcraStop.Modules.Habits.Presentation.Models;

public sealed class HabitDisciplineRankItem
{
    public HabitDisciplineRankItem(HabitDisciplineRankDto dto)
    {
        Position = dto.Position;
        Name = dto.Name;
        Score = dto.Score;
        ScoreProgress = Math.Clamp(dto.Score / 100d, 0d, 1d);
        ScoreText = $"{dto.Score:0}";
        ConsistencyText = $"{dto.ConsistencyRate:P0}";
        FailureText = $"{dto.FailureRate:P0}";
        TrendText = dto.Trend > 0
            ? $"+{dto.Trend:P0}"
            : $"{dto.Trend:P0}";
        StreakText = $"{dto.CurrentStreak} días";
        ProgressText = $"{dto.CompletedDays}/{dto.ExpectedDays} cumplidos";
        Status = dto.Status;
        AccentColor = dto.IsAtRisk
            ? Color.FromArgb("#EF5B5B")
            : dto.Score >= 85
                ? Color.FromArgb("#34C77B")
                : dto.Trend > .15
                    ? Color.FromArgb("#4F8CFF")
                    : Color.FromArgb("#F4B740");
    }

    public int Position { get; }
    public string PositionText => $"#{Position}";
    public string Name { get; }
    public double Score { get; }
    public double ScoreProgress { get; }
    public string ScoreText { get; }
    public string ConsistencyText { get; }
    public string FailureText { get; }
    public string TrendText { get; }
    public string StreakText { get; }
    public string ProgressText { get; }
    public string Status { get; }
    public Color AccentColor { get; }
}
