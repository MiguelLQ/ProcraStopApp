

using ProcraStop.Domain.Habits.Entities;
using ProcraStop.Domain.Habits.Enums;
using ProcraStop.Domain.Habits.ValueObjects;
using ProcraStop.Infrastructure.Habits.Persistence.Entities;

namespace ProcraStop.Infrastructure.Habits.Persistence.Mappings
{
    public static class HabitMapper
    {
        public static Habit ToDomain(HabitEntity entity)
        {
            //Console.WriteLine($"🛠 ENTITY ID: {entity.Id}");
            var target = HabitTarget.Create(entity.TargetValue);

            var days = entity.DaysOfWeek
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(d => (DayOfWeek)int.Parse(d));

            var schedule = HabitSchedule.Create(days);

            var rule = HabitCompletionRule.Create(entity.RequiredPercentage);

            var habit = Habit.Rehydrate(
                entity.Id,
                entity.Name,
                (HabitUnit)entity.Unit,
                target,
                schedule,
                rule,
                entity.Weight > 0 ? entity.Weight : 1.0,
                Enum.IsDefined(entity.TrackingType) && entity.TrackingType != 0
                    ? entity.TrackingType
                    : HabitTrackingType.Quantitative, // editbycodex: compatibilidad con filas antiguas guardadas en cero.
                entity.IsActive);

            //Console.WriteLine($"🧠 DOMAIN ID: {habit.Id}");
            return habit;
        }

        public static HabitEntity ToEntity(Habit habit)
        {
            return new HabitEntity
            {
                Id = habit.Id,
                Name = habit.Name,
                Unit = (int)habit.Unit,
                TargetValue = habit.Target.Value,
                DaysOfWeek = string.Join(",", habit.Schedule.Days.Select(d => (int)d)),
                RequiredPercentage = habit.CompletionRule.RequiredPercentage,
                Weight = habit.Weight,
                TrackingType = habit.TrackingType, // editbycodex: preserva el tipo real al persistir.
                IsActive = habit.IsActive
            };
        }
    }
}
