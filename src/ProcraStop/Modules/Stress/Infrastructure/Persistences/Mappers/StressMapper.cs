
using ProcraStop.Modules.Stress.Domain.Entities;
using ProcraStop.Modules.Stress.Domain.Enums;
using ProcraStop.Modules.Stress.Infrastructure.Persistences.Entities;

namespace ProcraStop.Modules.Stress.Infrastructure.Persistences.Mappers
{
    public static class StressMapper
    {
        public static StressRecordEntity ToEntity(StressRecord domain)
        => new()
        {
            Id = domain.Id,
            Level = domain.Level,
            Source = (int)domain.Source,
            Timestamp = domain.Timestamp
        };

        public static StressRecord ToDomain(StressRecordEntity entity)
        {
            return StressRecord.Restore(
                entity.Id,
                entity.Level,
                (StressSource)entity.Source,
                entity.Timestamp
            );
        }
    }
}
