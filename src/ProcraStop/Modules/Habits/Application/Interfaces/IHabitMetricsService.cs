
using ProcraStop.Modules.Habits.Application.DTOs;
using ProcraStop.Modules.Habits.Domain.Entities;

namespace ProcraStop.Modules.Habits.Application.Interfaces
{
    public interface IHabitMetricsService
    {
        // Calcula señales clave a partir de hábitos y logs recientes
        HabitSignals Calculate(
            IReadOnlyCollection<Habit> habits,
            IReadOnlyCollection<HabitLog> logs,
            DateTime today);
    }
}
