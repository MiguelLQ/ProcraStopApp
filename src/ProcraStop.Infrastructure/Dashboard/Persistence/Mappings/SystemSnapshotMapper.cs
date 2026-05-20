
using ProcraStop.Application.Dashboard.Models.Historial;
using ProcraStop.Infrastructure.Dashboard.Persistence.Entities;

namespace ProcraStop.Infrastructure.Dashboard.Persistence.Mappings
{
    public static class SystemSnapshotMapper
    {
        public static SystemSnapshot ToDomain(SystemSnapshotEntity entity)
        {
            return new SystemSnapshot
            {
                Timestamp = entity.Timestamp,
                Stress = entity.Stress,
                Performance = entity.Performance,
                Behavior = entity.Behavior
            };
        }

        public static SystemSnapshotEntity ToEntity(SystemSnapshot model)
        {
            return new SystemSnapshotEntity
            {
                Timestamp = model.Timestamp,
                Stress = model.Stress,
                Performance = model.Performance,
                Behavior = model.Behavior
            };
        }
    }
}
