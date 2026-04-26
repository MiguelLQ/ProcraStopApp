

using ProcraStop.Modules.Habits.Domain.Enums;
using ProcraStop.Modules.Habits.Domain.ValueObjects;
namespace ProcraStop.Modules.Habits.Domain.Entities
{
    public sealed class Habit
    {

        public Guid Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public HabitUnit Unit { get; private set; }

        public HabitTarget Target { get; private set; } = default!;

        public HabitSchedule Schedule { get; private set; } = default!;

        public HabitCompletionRule CompletionRule { get; private set; } = default!;
        public double Weight { get; private set; }
        public bool IsActive { get; private set; }
        public HabitTrackingType TrackingType { get; private set; }
        private Habit() { } // ORM
        // constructor privado para forzar el uso del método de fábrica y garantizar la creación de objetos válidos
        private Habit(
            Guid id,
            string name,
            HabitUnit unit,
            HabitTarget target,
            HabitSchedule schedule,
            HabitCompletionRule completionRule,
            double weight,
            HabitTrackingType trackingType)
        {
            Id = id;
            SetName(name);
            Unit = unit;
            Target = target;
            Schedule = schedule;
            CompletionRule = completionRule;
            Weight = weight;
            TrackingType = trackingType;
            IsActive = true;
        }
        //
        public static Habit Rehydrate(
            Guid id,
            string name,
            HabitUnit unit,
            HabitTarget target,
            HabitSchedule schedule,
            HabitCompletionRule completionRule,
            double weight,
            HabitTrackingType trackingType,
            bool isActive)
            {
                var habit = new Habit(id, name, unit, target, schedule, completionRule, weight, trackingType);

                if (!isActive)
                    habit.Deactivate();

                return habit;
            }
        // metodo de fábrica para crear hábitos de forma controlada,
        // asegurando que se cumplan las invariantes del dominio
        public static Habit Create(
            string name,
            HabitUnit unit,
            HabitTarget target,
            HabitSchedule schedule,
            HabitCompletionRule completionRule,
            HabitTrackingType trackingType,
            double weight)
        {
            return new Habit(
                Guid.NewGuid(),
                name,
                unit,
                target,
                schedule,
                completionRule,
                weight,
                trackingType);
        }
        // método para calcular el progreso del hábito en una fecha específica, 
        public double CalculateProgress(IEnumerable<HabitLog> logs, DateTime date)
        {
            if (logs is null || Target.Value == 0)
                return 0;

            var total = GetTotalValueForDate(logs, date);

            return Math.Min((double)total / Target.Value, 1.0);
        }
        // método para verificar si el hábito se ha completado en una fecha específica,
        public bool IsCompleted(IEnumerable<HabitLog> logs, DateTime date)
        {
            if (logs is null)
                return false;

            var total = GetTotalValueForDate(logs, date);

            return CompletionRule.IsCompleted(total, Target.Value);
        }
        // método privado para obtener el valor total registrado en una fecha específica,
        // utilizado internamente para calcular progreso y completitud
        private int GetTotalValueForDate(IEnumerable<HabitLog> logs, DateTime date)
        {
            return logs
                .Where(l => l.Date.Date == date.Date)
                .Sum(l => l.Value);
        }
        //BEHAVIOR

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre no puede estar vacío.");

            Name = name.Trim();
        }
        // métodos para actualizar las propiedades del hábito, con validaciones para mantener la consistencia del objeto
        public void UpdateTarget(HabitTarget target)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
        // métodos para actualizar las propiedades del hábito, con validaciones para mantener la consistencia del objeto
        public void UpdateSchedule(HabitSchedule schedule)
        {
            Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
        }
        // metodo para actualizar la regla de completitud del hábito, con validación para evitar asignar un valor nulo
        public void UpdateCompletionRule(HabitCompletionRule rule)
        {
            CompletionRule = rule ?? throw new ArgumentNullException(nameof(rule));
        }
        // métodos para activar o desactivar el hábito, permitiendo controlar su estado sin eliminarlo
        public void Activate() => IsActive = true;

        public void Deactivate() => IsActive = false;

        // CORE DOMAIN LOGIC (permitido)
        // metodo para verificar si el hábito está programado para una fecha específica,
        // delegando la lógica al objeto de valor Schedule
        public bool IsScheduledFor(DateTime date)
            => Schedule.IsScheduledFor(date);
        // metodo para verificar si el hábito se ha completado según la regla de completitud,
        public bool IsCompleted(int value)
            => CompletionRule.IsCompleted(value, Target.Value);
        //
        private readonly HashSet<DayOfWeek> _days;

        public IReadOnlyCollection<DayOfWeek> Days => _days;
    }
}
