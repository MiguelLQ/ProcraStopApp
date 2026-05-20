
namespace ProcraStop.Domain.Habits.ValueObjects
{
    public sealed class HabitTarget
    {
        // El valor objetivo para el hábito (por ejemplo, 30 minutos, 10 repeticiones, etc.)
        public int Value { get; }
        // El constructor es privado para forzar el uso del método de fábrica
        private HabitTarget(int value)
        {
            Value = value;
        }
        // Método de fábrica para crear una instancia de HabitTarget con validación
        public static HabitTarget Create(int value)
        {
            if (value <= 0)
                throw new ArgumentException("El valor objetivo debe ser mayor que 0.");

            return new HabitTarget(value);
        }
    }
}
