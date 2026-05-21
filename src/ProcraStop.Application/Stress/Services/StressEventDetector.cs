

using ProcraStop.Application.Habits.DTOs;
using ProcraStop.Application.Stress.Abstractions;
using ProcraStop.Domain.Stress.Entities;
using ProcraStop.Domain.Stress.Enums;

namespace ProcraStop.Application.Stress.Services
{
    public class StressEventDetector
    {
        private readonly IStressRepository _repo;

        public StressEventDetector(IStressRepository repo)
        {
            _repo = repo;
        }

        public async Task EvaluateAndRegisterAsync(
            TaskSignals tasks,
            HabitSignals habits)
        {
            // 
            var history = await _repo.GetRecentAsync(7);
            var last = history
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefault();

            var systemStress = CalculateSystemStress(tasks, habits);
            
            //  1. evitar duplicados cercanos
            if (last != null)
            {
                var timeDiff = (DateTime.UtcNow - last.Timestamp).TotalMinutes;

                if (timeDiff < 30)
                    return;
            }
            var todaySystem = history.Any(x =>
                x.Source == StressSource.System &&
                x.Timestamp.Date == DateTime.UtcNow.Date);
            if (todaySystem && last != null)
            {
                var delta = Math.Abs(systemStress - last.Level);

                if (delta < 0.2) // umbral configurable
                    return;
            }
            //  2. detectar evento significativo
            if (IsSignificantChange(last, systemStress, tasks, habits))
            {
                await _repo.AddAsync(
                    new StressRecord(systemStress, StressSource.System)
                );
            }
        }

        private bool IsSignificantChange(
            StressRecord? last,
            double current,
            TaskSignals tasks,
            HabitSignals habits)
        {
            if (last is null)
                return true;

            var delta = Math.Abs(current - last.Level);

            //  condición principal
            if (delta > 0.15)
                return true;

            //  condiciones de negocio
            if (tasks.OverdueCount >= 5)
                return true;

            if (habits.ConsistencyScore < 0.4)
                return true;

            return false;
        }

        private double CalculateSystemStress(TaskSignals tasks, HabitSignals habits)
        {
            var taskLoad =
                (tasks.OverdueCount * 0.5) +
                (tasks.PendingCount * 0.2) +
                (tasks.TotalEffort * 0.3);

            var normalizedTasks = Math.Clamp(taskLoad / 100.0, 0, 1);

            var habitInstability = 1 - habits.ConsistencyScore;

            return Math.Clamp(
                (0.7 * normalizedTasks) + (0.3 * habitInstability),
                0, 1
            );
        }
    }
}
