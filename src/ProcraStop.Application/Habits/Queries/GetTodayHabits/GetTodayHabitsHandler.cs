using ProcraStop.Application.Habits.Abstractions;
using ProcraStop.Application.Habits.DTOs;
using ProcraStop.Domain.Habits.Entities;

namespace ProcraStop.Application.Habits.Queries.GetTodayHabits;

// editbycodex: compone agenda diaria y rachas desde dos lecturas filtradas.
public sealed class GetTodayHabitsHandler
{
    private readonly IHabitRepository _habitRepository;
    private readonly IHabitLogRepository _logRepository;

    public GetTodayHabitsHandler(
        IHabitRepository habitRepository,
        IHabitLogRepository logRepository)
    {
        _habitRepository = habitRepository;
        _logRepository = logRepository;
    }

    public async Task<IReadOnlyList<HabitTodayItemDto>> Handle(
        GetTodayHabitsQuery query,
        CancellationToken ct = default)
    {
        var date = query.Date.Date;
        var habitsTask = _habitRepository.GetActiveAsync(ct);
        var logsTask = _logRepository.GetRangeAsync(date.AddDays(-90), date, ct);
        await Task.WhenAll(habitsTask, logsTask);

        var habits = await habitsTask;
        var logs = await logsTask;
        var logsByHabit = logs
            .GroupBy(x => x.HabitId)
            .ToDictionary(x => x.Key, x => (IReadOnlyList<HabitLog>)x.ToList());

        return habits
            .Where(habit => habit.IsScheduledFor(date))
            .Select(habit =>
            {
                logsByHabit.TryGetValue(habit.Id, out var habitLogs);
                habitLogs ??= Array.Empty<HabitLog>();
                return BuildItem(habit, habitLogs, date);
            })
            .OrderBy(x => x.IsCompleted)
            .ThenByDescending(x => x.CurrentStreak)
            .ThenBy(x => x.Name)
            .ToList();
    }

    public async Task<HabitTodayItemDto?> HandleOne(
        Guid habitId,
        DateTime date,
        CancellationToken ct = default)
    {
        date = date.Date;
        var habit = await _habitRepository.GetByIdAsync(habitId, ct);
        if (habit is null || !habit.IsActive || !habit.IsScheduledFor(date))
            return null;

        var logs = await _logRepository.GetRangeForHabitAsync(
            habitId,
            date.AddDays(-90),
            date,
            ct);

        return BuildItem(habit, logs, date);
    }

    private static HabitTodayItemDto BuildItem(
        Habit habit,
        IReadOnlyList<HabitLog> logs,
        DateTime date)
    {
        var valuesByDate = logs
            .GroupBy(log => log.Date.Date)
            .ToDictionary(group => group.Key, group => group.Sum(log => log.Value));
        valuesByDate.TryGetValue(date.Date, out var currentValue);

        return new HabitTodayItemDto
        {
            Id = habit.Id,
            Name = habit.Name,
            Unit = habit.Unit,
            TrackingType = habit.TrackingType,
            TargetValue = habit.Target.Value,
            CurrentValue = currentValue,
            Progress = habit.Target.Value == 0
                ? 0
                : Math.Min((double)currentValue / habit.Target.Value, 1.0),
            IsCompleted = habit.IsCompleted(currentValue),
            CurrentStreak = CalculateStreak(habit, valuesByDate, date),
            ScheduleText = BuildScheduleText(habit)
        };
    }

    private static int CalculateStreak(
        Habit habit,
        IReadOnlyDictionary<DateTime, int> valuesByDate,
        DateTime date)
    {
        var streak = 0;
        var cursor = date.Date;

        // editbycodex: el dia actual incompleto no rompe una racha mientras aun puede cumplirse.
        if (cursor == DateTime.Today &&
            habit.IsScheduledFor(cursor) &&
            !IsCompleted(habit, valuesByDate, cursor))
            cursor = cursor.AddDays(-1);

        for (var inspected = 0; inspected < 90; inspected++, cursor = cursor.AddDays(-1))
        {
            if (!habit.IsScheduledFor(cursor))
                continue;
            if (!IsCompleted(habit, valuesByDate, cursor))
                break;
            streak++;
        }

        return streak;
    }

    private static bool IsCompleted(
        Habit habit,
        IReadOnlyDictionary<DateTime, int> valuesByDate,
        DateTime date)
    {
        valuesByDate.TryGetValue(date.Date, out var value);
        return habit.IsCompleted(value);
    }

    private static string BuildScheduleText(Habit habit)
    {
        return habit.Schedule.Days.Count == 7
            ? "Todos los días"
            : $"{habit.Schedule.Days.Count} días por semana";
    }
}
