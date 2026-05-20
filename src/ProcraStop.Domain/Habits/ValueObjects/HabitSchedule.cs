

namespace ProcraStop.Domain.Habits.ValueObjects
{
    public sealed class HabitSchedule
    {
        private readonly HashSet<DayOfWeek> _days;

        private HabitSchedule(IEnumerable<DayOfWeek> days)
        {
            _days = new HashSet<DayOfWeek>(days);
        }

        public static HabitSchedule Create(IEnumerable<DayOfWeek> days)
        {
            if (days == null || !days.Any())
                throw new ArgumentException("Schedule debe contener al menos un día");

            return new HabitSchedule(days);
        }

        public IReadOnlyCollection<DayOfWeek> Days => _days;

        public bool IsScheduledFor(DateTime date)
        {
            return _days.Contains(date.DayOfWeek);
        }
    }
}
