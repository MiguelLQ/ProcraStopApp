

using ProcraStop.Domain.Habits.Entities;
using ProcraStop.Domain.Habits.Enums;
using ProcraStop.Domain.Habits.ValueObjects;
using ProcraStop.Infrastructure.Habits.Persistence.Entities;

namespace ProcraStop.Modules.Habits.Infraestructure.Mappers
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
                entity.Weight,
                (HabitTrackingType)entity.TrackingType,
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
                IsActive = habit.IsActive
            };
        }
    }
}
