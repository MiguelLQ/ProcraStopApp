

using ProcraStop.Modules.Habits.Domain.Entities;
using ProcraStop.Modules.Habits.Infraestructure.Persistence.Entities;

namespace ProcraStop.Modules.Habits.Infraestructure.Mappers
{
    public static class HabitLogMapper
    {
        public static HabitLog ToDomain(HabitLogEntity entity)
        {
            return HabitLog.Create(
                entity.HabitId,
                entity.Date,
                entity.Value,
                entity.Notes);
        }

        public static HabitLogEntity ToEntity(HabitLog log)
        {
            return new HabitLogEntity
            {
                Id = log.Id,
                HabitId = log.HabitId,
                Date = log.Date,
                Value = log.Value,
                Notes = log.Notes
            };
        }
    }
}
