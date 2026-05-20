

using ProcraStop.Domain.Stress.Enums;

namespace ProcraStop.Infrastructure.Stress.Persistence.Entities
{
    public class StressRecordEntity
    {
        public Guid Id { get; set; }
        public double Level { get; set; }
        public StressSource Source { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
