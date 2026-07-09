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
        var habits = await _habitRepository.GetActiveAsync(ct);
        var logs = await _logRepository.GetRangeAsync(date.AddDays(-90), date, ct);
        var logsByHabit = logs
            .GroupBy(x => x.HabitId)
            .ToDictionary(x => x.Key, x => (IReadOnlyList<HabitLog>)x.ToList());

        return habits
            .Where(habit => habit.IsScheduledFor(date))
            .Select(habit =>
            {
                logsByHabit.TryGetValue(habit.Id, out var habitLogs);
                habitLogs ??= Array.Empty<HabitLog>();
                var currentValue = habitLogs
                    .Where(log => log.Date.Date == date)
                    .Sum(log => log.Value);

                return new HabitTodayItemDto
                {
                    Id = habit.Id,
                    Name = habit.Name,
                    Unit = habit.Unit,
                    TrackingType = habit.TrackingType,
                    TargetValue = habit.Target.Value,
                    CurrentValue = currentValue,
                    Progress = habit.CalculateProgress(habitLogs, date),
                    IsCompleted = habit.IsCompleted(habitLogs, date),
                    CurrentStreak = CalculateStreak(habit, habitLogs, date),
                    ScheduleText = BuildScheduleText(habit)
                };
            })
            .OrderBy(x => x.IsCompleted)
            .ThenByDescending(x => x.CurrentStreak)
            .ThenBy(x => x.Name)
            .ToList();
    }

    private static int CalculateStreak(Habit habit, IReadOnlyList<HabitLog> logs, DateTime date)
    {
        var streak = 0;
        var cursor = date.Date;

        // editbycodex: el dia actual incompleto no rompe una racha mientras aun puede cumplirse.
        if (cursor == DateTime.Today && habit.IsScheduledFor(cursor) && !habit.IsCompleted(logs, cursor))
            cursor = cursor.AddDays(-1);

        for (var inspected = 0; inspected < 90; inspected++, cursor = cursor.AddDays(-1))
        {
            if (!habit.IsScheduledFor(cursor))
                continue;
            if (!habit.IsCompleted(logs, cursor))
                break;
            streak++;
        }

        return streak;
    }

    private static string BuildScheduleText(Habit habit)
    {
        return habit.Schedule.Days.Count == 7
            ? "Todos los días"
            : $"{habit.Schedule.Days.Count} días por semana";
    }
}
