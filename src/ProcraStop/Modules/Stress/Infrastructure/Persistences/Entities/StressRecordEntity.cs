

using ProcraStop.Modules.Stress.Domain.Enums;

namespace ProcraStop.Modules.Stress.Infrastructure.Persistences.Entities
{
    public class StressRecordEntity
    {
        public Guid Id { get; set; }
        public double Level { get; set; }
        public StressSource Source { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
