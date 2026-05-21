

using ProcraStop.Application.Habits.DTOs;
using ProcraStop.Application.Habits.Interfaces;
using ProcraStop.Domain.Habits.Entities;

namespace ProcraStop.Application.Habits.Services
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
            {
                return HabitSignals.Empty();
            }

            var last7Days = Enumerable.Range(0, 7)
                .Select(i => today.Date.AddDays(-i))
                .ToList();

            var logsByHabit = logs
                .GroupBy(l => l.HabitId)
                .ToDictionary(g => g.Key, g => g.ToList());
            // para acceso rápido a logs por hábito y fecha
            var logsByHabitAndDate = logs
                .GroupBy(l => (l.HabitId, Date: l.Date.Date))
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );
            var streaks = new List<int>();
            double totalExpected = 0;
            double totalCompleted = 0;

            double effortAccumulated = 0;
            double effortCount = 0;

            int globalStreak = int.MaxValue;
            // fallos
            double totalFailed = 0;
            double failureDays = 0;

            int globalFailureStreak = int.MaxValue;

            foreach (var habit in habits.Where(h => h.IsActive))
            {
                logsByHabit.TryGetValue(habit.Id, out var habitLogs);
                habitLogs ??= new List<HabitLog>();

                int streak = 0;
                bool streakBroken = false;

                double weeklyExpected = 0;
                double weeklyCompleted = 0;
                // fallos
                int failureStreak = 0;
                bool failureBroken = false;

                foreach (var date in last7Days)
                {
                    if (!habit.IsScheduledFor(date))
                        continue;

                    weeklyExpected += habit.Weight;
                    totalExpected += habit.Weight;

                    IEnumerable<HabitLog> dayLogs =
                    logsByHabitAndDate.TryGetValue((habit.Id, date.Date), out var logsForDay)
                        ? logsForDay
                        : Array.Empty<HabitLog>();
                    var completed = habit.IsCompleted(dayLogs, date);

                    if (completed)
                    {
                        totalCompleted += habit.Weight;
                        weeklyCompleted += habit.Weight;
                    }
                    if (!completed)
                    {
                        totalFailed += habit.Weight;
                        failureDays += habit.Weight;
                    }
                    double effort;

                    effort = completed ? 1.0 : 0.0;
                  

                    effortAccumulated += effort * habit.Weight;
                    effortCount += habit.Weight;
                    // fallos (desde hoy hacia atrás)
                    if (!failureBroken)
                    {
                        if (!completed) 
                            failureStreak++;
                        else
                            failureBroken = true;
                    }
                    // streak (desde hoy hacia atrás)
                    if (!streakBroken)
                    {
                        if (completed)
                            streak++;
                        else
                            streakBroken = true;
                    }
                }
                streaks.Add(streak);    
                // el streak global es el mínimo entre hábitos (consistencia real)
                globalStreak = Math.Min(globalStreak, streak);
                globalFailureStreak = Math.Min(globalFailureStreak, failureStreak);
            }
            // fallos - si el usuario tiene una racha de fallos muy alta, no queremos que el número
            // se descontrole y afecte otras métricas, por lo que lo limitamos a un máximo (ej. 100)
            if (globalFailureStreak == int.MaxValue)
                globalFailureStreak = 0;

            var failureRate = totalExpected == 0
                ? 0
                : totalFailed / totalExpected;  

            if (globalStreak == int.MaxValue)
                globalStreak = 0;
            var averageStreak = streaks.Count == 0
            ? 0
            : (int)Math.Round(streaks.Average());
            var bestStreak = streaks.Count == 0
            ? 0
            : streaks.Max();
            var consistency = totalExpected == 0 ? 0 : totalCompleted / totalExpected;

            var avgEffort = effortCount == 0 ? 0 : effortAccumulated / effortCount;
            // TEMP: ConsistencyScore actualmente usa ventana de 7 días.
            // En futuras versiones se ampliará a ventana mayor (30 días o rolling)
                    //var last30Days = Enumerable.Range(0, 30) // pensada a futuro
                    //    .Select(i => today.Date.AddDays(-i))
                    //    .ToList();
            var weeklyRate = totalExpected == 0
                ? 0
                : totalCompleted / totalExpected;
            return new HabitSignals
            {
                ConsistencyScore = Round(consistency),
                WeeklyCompletionRate = Round(weeklyRate),
                AverageEffort = Round(avgEffort),

                CurrentStreak = globalStreak,
                AverageStreak = averageStreak,
                BestStreak = bestStreak,

                FailureRate = Round(failureRate),
                FailureDays = Round(failureDays),
                FailureStreak = globalFailureStreak
            };
        }
        
        // metodo para redondear a 2 decimales
        private static double Round(double value)
            => Math.Round(value, 2);
    }
}
