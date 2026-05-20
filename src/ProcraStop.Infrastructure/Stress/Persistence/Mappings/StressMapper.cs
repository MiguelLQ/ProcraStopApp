

using ProcraStop.Domain.Stress.Entities;
using ProcraStop.Domain.Stress.Enums;
using ProcraStop.Infrastructure.Stress.Persistence.Entities;

namespace ProcraStop.Infrastructure.Stress.Persistence.Mappings
{
    public static class StressMapper
    {
        public static StressRecordEntity ToEntity(StressRecord domain)
        => new()
        {
            Id = domain.Id,
            Level = domain.Level,
            Source = domain.Source,
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
