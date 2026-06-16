

using ProcraStop.Core.Decisions.Signals;
using ProcraStop.Domain.Habits.Entities;

namespace ProcraStop.Application.Habits.Interfaces
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
