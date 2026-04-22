
namespace ProcraStop.Modules.Habits.Infraestructure.Persistence.Entities
{
    public class HabitLogEntity
    {
        public Guid Id { get; set; }

        public Guid HabitId { get; set; }

        public DateTime Date { get; set; }

        public int Value { get; set; }

        public string? Notes { get; set; }
    }
}
