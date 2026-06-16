

using ProcraStop.Core.Decisions.Enums;

namespace ProcraStop.Core.Decisions.Signals
{
    public class StressSignals
    {
        public double CurrentLevel { get; init; }        // 0–1
        public StressTrend Trend { get; init; }          // comportamiento
        public double RecoveryCapacity { get; init; }    // capacidad de recuperarse
        public double LoadTolerance { get; init; }       // cuánto puede soportar

        public static StressSignals Empty() => new()
        {
            CurrentLevel = 0,
            Trend = StressTrend.Stable,
            RecoveryCapacity = 1,
            LoadTolerance = 1
        };
    }
}
