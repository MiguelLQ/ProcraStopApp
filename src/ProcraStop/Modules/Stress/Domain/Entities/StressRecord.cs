
using ProcraStop.Modules.Stress.Domain.Enums;

namespace ProcraStop.Modules.Stress.Domain.Entities
{
    public sealed class StressRecord
    {
        public Guid Id { get; private set; }
        public double Level { get; private set; } // 0–1
        public StressSource Source { get; private set; }
        public DateTime Timestamp { get; private set; }

        private StressRecord() { }

        public StressRecord(double level, StressSource source)
        {
            Level = Math.Clamp(level, 0, 1);
            Source = source;
            Timestamp = DateTime.UtcNow;
        }
        // factory para restaurar um registro de estresse a partir de dados persistidos
        public static StressRecord Restore(
            Guid id,
            double level,
            StressSource source,
            DateTime timestamp)
        {
            var record = new StressRecord(level, source);

            record.Id = id;
            record.Timestamp = timestamp;

            return record;
        }
    }
}
