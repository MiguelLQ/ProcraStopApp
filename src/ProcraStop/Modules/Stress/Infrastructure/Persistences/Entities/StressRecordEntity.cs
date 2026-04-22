

namespace ProcraStop.Modules.Stress.Infrastructure.Persistences.Entities
{
    public class StressRecordEntity
    {
        public Guid Id { get; set; }
        public double Level { get; set; }
        public int Source { get; set; } // enum como int
        public DateTime Timestamp { get; set; }
    }
}
