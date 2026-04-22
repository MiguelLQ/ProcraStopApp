
namespace ProcraStop.Modules.Habits.Infraestructure.Persistence.Entities
{
    public class HabitEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Unit { get; set; }

        public int TargetValue { get; set; }

        // guardamos días como string simple (ej: "1,2,3")
        public string DaysOfWeek { get; set; } = string.Empty;

        public double RequiredPercentage { get; set; }

        public bool IsActive { get; set; }
    }
}
