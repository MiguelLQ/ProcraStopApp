

namespace ProcraStop.Modules.Habits.Domain.Entities
{
    public sealed class HabitLog
    {
        public Guid Id { get; private set; }

        public Guid HabitId { get; private set; }

        public DateTime Date { get; private set; }

        public int Value { get; private set; }

        public string? Notes { get; private set; }

        private HabitLog() { } // ORM
        // constructor privado para garantizar la creación controlada a través del método estático Create
        private HabitLog(Guid id, Guid habitId, DateTime date, int value, string? notes)
        {
            Id = id;
            HabitId = habitId;
            Date = date.Date;
            SetValue(value);
            Notes = notes;
        }
        // Método de fábrica para crear una nueva instancia de HabitLog con validaciones
        public static HabitLog Create(Guid habitId, DateTime date, int value, string? notes = null)
        {
            if (habitId == Guid.Empty)
                throw new ArgumentException("HabitId no puede estar vacía.");

            return new HabitLog(Guid.NewGuid(), habitId, date, value, notes);
        }
        // Método para actualizar el valor del hábito con validación
        public void SetValue(int value)
        {
            if (value < 0)
                throw new ArgumentException("El valor no puede ser negativo");

            Value = value;
        }
        // Método para actualizar las notas del hábito
        public void AddNotes(string? notes)
        {
            Notes = notes;
        }
    }
}
