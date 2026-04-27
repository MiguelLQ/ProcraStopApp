

namespace ProcraStop.Modules.Habits.Presentation.Models
{
    public class HabitItemViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public int Target { get; set; }

        public double Weight { get; set; }

        public bool IsActive { get; set; }
    }
}
