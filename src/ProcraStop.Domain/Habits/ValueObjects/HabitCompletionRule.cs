

namespace ProcraStop.Domain.Habits.ValueObjects
{
    public sealed class HabitCompletionRule
    {
        // El porcentaje requerido para considerar un hábito como completado (entre 0 y 1).
        public double RequiredPercentage { get; }
        // Constructor privado para asegurar que se use el método de creación estático.
        private HabitCompletionRule(double requiredPercentage)
        {
            RequiredPercentage = requiredPercentage;
        }
        // Método de creación estático para validar el porcentaje antes de crear la instancia.
        public static HabitCompletionRule Create(double requiredPercentage)
        {
            if (requiredPercentage <= 0 || requiredPercentage > 1)
                throw new ArgumentException("El porcentaje debe estar entre 0 y 1.");

            return new HabitCompletionRule(requiredPercentage);
        }
        // Método para determinar si un hábito se considera completado basado en el valor actual y el objetivo.
        public bool IsCompleted(int value, int target)
        {
            if (target <= 0) return false;

            var ratio = (double)value / target;
            return ratio >= RequiredPercentage;
        }
        //
        public static HabitCompletionRule AtLeast() => new HabitCompletionRule(1.0);

        public static HabitCompletionRule Exact() => new HabitCompletionRule(1.0);

        public static HabitCompletionRule Any() => new HabitCompletionRule(0.1);
    }
}
