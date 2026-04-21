
namespace ProcraStop.Modules.Habits.Domain.ValueObjects
{
    public sealed class HabitSchedule
    {
        // usa un IReadOnlyCollection para garantizar que la colección de días no
        // pueda ser modificada desde fuera de la clase
        public IReadOnlyCollection<DayOfWeek> Days { get; }
        // el constructor es privado para forzar el uso del método de fábrica, que puede incluir validaciones
        private HabitSchedule(IEnumerable<DayOfWeek> days)
        {
            Days = days.Distinct().ToList().AsReadOnly();
        }
        // el método de fábrica permite crear una instancia de HabitSchedule con validaciones adicionales
        public static HabitSchedule Create(IEnumerable<DayOfWeek> days)
        {
            if (days == null || !days.Any())
                throw new ArgumentException("Schedule must contain at least one day.");

            return new HabitSchedule(days);
        }
        // este método verifica si el hábito está programado para un día específico 
        public bool IsScheduledFor(DateTime date)
        {
            return Days.Contains(date.DayOfWeek);
        }
    }
}
