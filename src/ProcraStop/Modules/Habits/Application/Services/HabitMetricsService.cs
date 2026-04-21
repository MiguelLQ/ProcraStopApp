
using ProcraStop.Modules.Habits.Application.DTOs;
using ProcraStop.Modules.Habits.Application.Interfaces;
using ProcraStop.Modules.Habits.Domain.Entities;

namespace ProcraStop.Modules.Habits.Application.Services
{
    public class HabitMetricsService: IHabitMetricsService
    {
        // metodo principal para calcular señales a partir de hábitos y logs
        public HabitSignals Calculate(
       IReadOnlyCollection<Habit> habits,
       IReadOnlyCollection<HabitLog> logs,
       DateTime today)
        {
            if (!habits.Any())
                return Empty();

            var last7Days = Enumerable.Range(0, 7)
                .Select(i => today.Date.AddDays(-i))
                .ToList();

            var logsByHabit = logs
                .GroupBy(l => l.HabitId)
                .ToDictionary(g => g.Key, g => g.ToList());

            double totalExpected = 0;
            double totalCompleted = 0;

            double effortAccumulated = 0;
            double effortCount = 0;

            int globalStreak = int.MaxValue;

            foreach (var habit in habits.Where(h => h.IsActive))
            {
                logsByHabit.TryGetValue(habit.Id, out var habitLogs);
                habitLogs ??= new List<HabitLog>();

                int streak = 0;
                bool streakBroken = false;

                int weeklyExpected = 0;
                int weeklyCompleted = 0;

                foreach (var date in last7Days)
                {
                    if (!habit.IsScheduledFor(date))
                        continue;

                    weeklyExpected++;
                    totalExpected++;

                    var log = habitLogs.FirstOrDefault(l => l.Date == date);

                    var value = log?.Value ?? 0;

                    var completed = habit.IsCompleted(value);

                    if (completed)
                    {
                        totalCompleted++;
                        weeklyCompleted++;
                    }

                    // esfuerzo relativo
                    var effortRatio = Math.Min((double)value / habit.Target.Value, 1.0);
                    effortAccumulated += effortRatio;
                    effortCount++;

                    // streak (desde hoy hacia atrás)
                    if (!streakBroken)
                    {
                        if (completed)
                            streak++;
                        else
                            streakBroken = true;
                    }
                }

                // el streak global es el mínimo entre hábitos (consistencia real)
                globalStreak = Math.Min(globalStreak, streak);
            }

            if (globalStreak == int.MaxValue)
                globalStreak = 0;

            var consistency = totalExpected == 0 ? 0 : totalCompleted / totalExpected;

            var avgEffort = effortCount == 0 ? 0 : effortAccumulated / effortCount;

            var weeklyRate = consistency; // en esta V1 coincide (puedes separarlo después)

            return new HabitSignals
            {
                ConsistencyScore = Round(consistency),
                CurrentStreak = globalStreak,
                AverageEffort = Round(avgEffort),
                WeeklyCompletionRate = Round(weeklyRate)
            };
        }
        // método para retornar señales vacías cuando no hay hábitos
        private static HabitSignals Empty() => new()
        {
            ConsistencyScore = 0,
            CurrentStreak = 0,
            AverageEffort = 0,
            WeeklyCompletionRate = 0
        };
        // metodo para redondear a 2 decimales
        private static double Round(double value)
            => Math.Round(value, 2);
    }
}
