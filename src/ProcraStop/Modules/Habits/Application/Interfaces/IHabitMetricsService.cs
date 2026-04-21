
using ProcraStop.Modules.Habits.Application.DTOs;
using ProcraStop.Modules.Habits.Domain.Entities;

namespace ProcraStop.Modules.Habits.Application.Interfaces
{
    public interface IHabitMetricsService
    {
        HabitSignals Calculate(
        IReadOnlyCollection<Habit> habits,
        IReadOnlyCollection<HabitLog> logs,
        DateTime today);
    }
}
