
using ProcraStop.Modules.Habits.Domain.Enums;

namespace ProcraStop.Modules.Habits.Application.Commands.CreateHabit
{
    public class CreateHabitCommand
    {
        public string Name { get; set; } = string.Empty;

        public HabitUnit Unit { get; set; }

        public int TargetValue { get; set; }

        public int[] DaysOfWeek { get; set; } = []; // 0-6 (domingo-sábado)
        public double RequiredPercentage { get; set; } // 0 - 1
    }
}
