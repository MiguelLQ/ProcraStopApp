using ProcraStop.Application.Habits.Abstractions;
using ProcraStop.Application.Habits.Interfaces;
using ProcraStop.Domain.Habits.Entities;
using ProcraStop.Domain.Habits.Enums;

namespace ProcraStop.Application.Habits.Analytics;

public sealed class HabitAnalyticsService
{
    private readonly IHabitRepository _habitRepository;
    private readonly IHabitLogRepository _logRepository;
    private readonly IHabitMetricsService _metricsService;

    public HabitAnalyticsService(
        IHabitRepository habitRepository,
        IHabitLogRepository logRepository,
        IHabitMetricsService metricsService)
    {
        _habitRepository = habitRepository;
        _logRepository = logRepository;
        _metricsService = metricsService;
    }

    public async Task<HabitAnalyticsDto> GetAsync(CancellationToken ct)
    {
        var today = DateTime.Today;
        var start = today.AddDays(-6);
        var habits = (await _habitRepository.GetActiveAsync(ct)).Where(h => h.IsActive).ToArray();
        var logs = await _logRepository.GetRangeAsync(start, today, ct);
        var signals = _metricsService.Calculate(habits, logs, today);
        var weekDays = Enumerable.Range(0, 7).Select(offset => start.AddDays(offset)).ToArray();
        var completedByDay = weekDays.Select(day => CountCompletedForDay(habits, logs, day)).ToArray();
        var expectedThisWeek = weekDays.Sum(day => habits.Count(h => h.IsScheduledFor(day)));
        var completedThisWeek = completedByDay.Sum();
        var scheduledToday = habits.Count(h => h.IsScheduledFor(today));
        var completedToday = CountCompletedForDay(habits, logs, today);
        var atRisk = habits.Count(h => IsAtRisk(h, logs, today));

        return new HabitAnalyticsDto
        {
            ActiveHabits = habits.Length,
            ScheduledToday = scheduledToday,
            CompletedToday = completedToday,
            ExpectedThisWeek = expectedThisWeek,
            CompletedThisWeek = completedThisWeek,
            AtRiskHabits = atRisk,
            BinaryHabits = habits.Count(h => h.TrackingType == HabitTrackingType.Binary),
            QuantitativeHabits = habits.Count(h => h.TrackingType == HabitTrackingType.Quantitative),
            CurrentStreak = signals.CurrentStreak,
            BestStreak = signals.BestStreak,
            ConsistencyRate = expectedThisWeek == 0 ? 0 : (double)completedThisWeek / expectedThisWeek,
            FailureRate = signals.FailureRate,
            WeeklyLabels = weekDays.Select(d => d.ToString("ddd")).ToArray(),
            WeeklyCompleted = completedByDay.Select(v => (double)v).ToArray(),
            StatusLabels = new[] { "Completados", "Pendientes" },
            StatusValues = new double[] { completedToday, Math.Max(0, scheduledToday - completedToday) },
            TypeLabels = new[] { "Binarios", "Cuantitativos" },
            TypeValues = new double[]
            {
                habits.Count(h => h.TrackingType == HabitTrackingType.Binary),
                habits.Count(h => h.TrackingType == HabitTrackingType.Quantitative)
            },
            RiskLabels = new[] { "En riesgo", "Estables" },
            RiskValues = new double[] { atRisk, Math.Max(0, habits.Length - atRisk) }
        };
    }

    private static int CountCompletedForDay(IEnumerable<Habit> habits, IReadOnlyCollection<HabitLog> logs, DateTime day)
    {
        return habits.Count(h => h.IsScheduledFor(day) && h.IsCompleted(logs.Where(l => l.HabitId == h.Id), day));
    }

    private static bool IsAtRisk(Habit habit, IReadOnlyCollection<HabitLog> logs, DateTime today)
    {
        var scheduledDays = Enumerable.Range(0, 7)
            .Select(offset => today.AddDays(-offset))
            .Where(habit.IsScheduledFor)
            .Take(3)
            .ToArray();

        return scheduledDays.Length > 0 &&
            scheduledDays.All(day => !habit.IsCompleted(logs.Where(l => l.HabitId == habit.Id), day));
    }
}
