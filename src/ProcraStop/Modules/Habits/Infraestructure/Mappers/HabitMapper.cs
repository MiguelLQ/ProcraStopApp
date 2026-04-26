

using ProcraStop.Modules.Habits.Domain.Entities;
using ProcraStop.Modules.Habits.Domain.Enums;
using ProcraStop.Modules.Habits.Domain.ValueObjects;
using ProcraStop.Modules.Habits.Infraestructure.Persistence.Entities;

namespace ProcraStop.Modules.Habits.Infraestructure.Mappers
{
    public static class HabitMapper
    {
        public static Habit ToDomain(HabitEntity entity)
        {
            var target = HabitTarget.Create(entity.TargetValue);

            var days = entity.DaysOfWeek
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(d => (DayOfWeek)int.Parse(d));

            var schedule = HabitSchedule.Create(days);

            var rule = HabitCompletionRule.Create(entity.RequiredPercentage);

            var habit = Habit.Create(
                entity.Name,
                (HabitUnit)entity.Unit,
                target,
                schedule,
                rule,
                (HabitTrackingType)entity.TrackingType,
                entity.Weight);

            // setear estado real
            if (!entity.IsActive)
                habit.Deactivate();

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
