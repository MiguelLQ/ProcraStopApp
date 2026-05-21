
namespace ProcraStop.Application.Habits.Commands.TrackHabit
{
    public class TrackHabitCommand
    {
        public Guid HabitId { get; set; }

        public DateTime Date { get; set; }

        public int Value { get; set; }

        public string? Notes { get; set; }
    }
}
