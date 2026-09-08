using ProcraStop.Application.Habits.Abstractions;
using ProcraStop.Domain.Habits.Entities;

namespace ProcraStop.Application.Habits.Ranking;

public sealed class HabitDisciplineRankingService
{
    private const int WindowDays = 14;
    private readonly IHabitRepository _habitRepository;
    private readonly IHabitLogRepository _logRepository;

    public HabitDisciplineRankingService(
        IHabitRepository habitRepository,
        IHabitLogRepository logRepository)
    {
        _habitRepository = habitRepository;
        _logRepository = logRepository;
    }

    public async Task<HabitDisciplineRankingDto> GetAsync(CancellationToken ct)
    {
        var today = DateTime.Today;
        var start = today.AddDays(-(WindowDays - 1));
        var habits = (await _habitRepository.GetActiveAsync(ct))
            .Where(h => h.IsActive)
            .ToArray();
        var logs = await _logRepository.GetRangeAsync(start, today, ct);

        var ranked = habits
            .Select(habit => BuildRank(habit, logs, today, start))
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.CurrentStreak)
            .ThenBy(item => item.Name)
            .Select((item, index) => item.WithPosition(index + 1))
            .ToArray();

        return new HabitDisciplineRankingDto
        {
            ActiveHabits = ranked.Length,
            EliteHabits = ranked.Count(h => h.Score >= 85),
            AtRiskHabits = ranked.Count(h => h.IsAtRisk),
            AverageScore = ranked.Length == 0 ? 0 : Math.Round(ranked.Average(h => h.Score), 0),
            Items = ranked
        };
    }

    private static HabitDisciplineRankDto BuildRank(
        Habit habit,
        IReadOnlyCollection<HabitLog> logs,
        DateTime today,
        DateTime start)
    {
        var scheduledDays = Enumerable.Range(0, WindowDays)
            .Select(offset => start.AddDays(offset))
            .Where(habit.IsScheduledFor)
            .ToArray();

        if (scheduledDays.Length == 0)
        {
            return CreateRank(habit, 0, 0, 0, 0, 0, 0, false, "Sin agenda reciente");
        }

        var habitLogs = logs.Where(l => l.HabitId == habit.Id).ToArray();
        var completedDays = scheduledDays.Count(day => habit.IsCompleted(habitLogs, day));
        var consistency = (double)completedDays / scheduledDays.Length;
        var failureRate = 1 - consistency;
        var currentStreak = CalculateCurrentStreak(habit, habitLogs, today);
        var trend = CalculateTrend(habit, habitLogs, today);
        var streakScore = Math.Min(currentStreak / 7d, 1d);
        var weightScore = Math.Clamp(habit.Weight / 3d, 0d, 1d);
        var score = (consistency * 58) + (streakScore * 24) + ((1 - failureRate) * 13) + (weightScore * 5);
        var roundedScore = Math.Round(Math.Clamp(score, 0, 100), 0);
        var isAtRisk = failureRate >= .5 || currentStreak == 0 && scheduledDays.Length >= 3;
        var status = BuildStatus(roundedScore, trend, isAtRisk);

        return CreateRank(
            habit,
            roundedScore,
            consistency,
            failureRate,
            trend,
            currentStreak,
            completedDays,
            isAtRisk,
            status,
            scheduledDays.Length);
    }

    private static int CalculateCurrentStreak(Habit habit, IReadOnlyCollection<HabitLog> logs, DateTime today)
    {
        var streak = 0;

        foreach (var day in Enumerable.Range(0, WindowDays).Select(offset => today.AddDays(-offset)))
        {
            if (!habit.IsScheduledFor(day))
                continue;

            if (!habit.IsCompleted(logs, day))
                break;

            streak++;
        }

        return streak;
    }

    private static double CalculateTrend(Habit habit, IReadOnlyCollection<HabitLog> logs, DateTime today)
    {
        var recent = CompletionRate(habit, logs, today.AddDays(-6), today);
        var previous = CompletionRate(habit, logs, today.AddDays(-13), today.AddDays(-7));

        return Math.Round(recent - previous, 2);
    }

    private static double CompletionRate(Habit habit, IReadOnlyCollection<HabitLog> logs, DateTime start, DateTime end)
    {
        var scheduled = Enumerable.Range(0, (end.Date - start.Date).Days + 1)
            .Select(offset => start.Date.AddDays(offset))
            .Where(habit.IsScheduledFor)
            .ToArray();

        return scheduled.Length == 0
            ? 0
            : (double)scheduled.Count(day => habit.IsCompleted(logs, day)) / scheduled.Length;
    }

    private static string BuildStatus(double score, double trend, bool isAtRisk)
    {
        if (isAtRisk)
            return "Requiere foco";

        if (score >= 85)
            return "Disciplina alta";

        if (trend > .15)
            return "En ascenso";

        if (score >= 65)
            return "Estable";

        return "Por consolidar";
    }

    private static HabitDisciplineRankDto CreateRank(
        Habit habit,
        double score,
        double consistency,
        double failureRate,
        double trend,
        int currentStreak,
        int completedDays,
        bool isAtRisk,
        string status,
        int expectedDays = 0)
    {
        return new HabitDisciplineRankDto
        {
            HabitId = habit.Id,
            Name = habit.Name,
            Score = score,
            ConsistencyRate = Math.Round(consistency, 2),
            FailureRate = Math.Round(failureRate, 2),
            Trend = trend,
            CurrentStreak = currentStreak,
            CompletedDays = completedDays,
            ExpectedDays = expectedDays,
            IsAtRisk = isAtRisk,
            Status = status
        };
    }
}
