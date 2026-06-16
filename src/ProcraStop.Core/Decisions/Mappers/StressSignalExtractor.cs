
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Enums;
using ProcraStop.Core.Decisions.Signals;

namespace ProcraStop.Core.Decisions.Mappers
{
    public static class StressSignalExtractor
    {
        public static IEnumerable<UnifiedSignal> Extract(StressSignals stress)
        {
            var signals = new List<UnifiedSignal>();

            // 1. Estado actual
            signals.Add(new UnifiedSignal
            {
                Name = "Stress Level",
                Category = "Stress",
                Score = stress.CurrentLevel,
                Type = GetStressType(stress.CurrentLevel),
                Source = "Stress"
            });

            // 2. Tendencia (MUY importante en tu modelo)
            signals.Add(new UnifiedSignal
            {
                Name = "Stress Trend",
                Category = "Trajectory",
                Score = TrendScore(stress.Trend),
                Type = stress.Trend == StressTrend.Rising
                    ? DecisionType.Warning
                    : DecisionType.Info,
                Source = "Stress"
            });

            // 3. Recovery capacity (resiliencia)
            signals.Add(new UnifiedSignal
            {
                Name = "Recovery Capacity",
                Category = "Resilience",
                Score = 1 - stress.RecoveryCapacity,
                Type = stress.RecoveryCapacity < 0.5
                    ? DecisionType.Warning
                    : DecisionType.Success,
                Source = "Stress"
            });

            // 4. Load tolerance (capacidad futura)
            signals.Add(new UnifiedSignal
            {
                Name = "Load Tolerance",
                Category = "Capacity",
                Score = 1 - stress.LoadTolerance,
                Type = stress.LoadTolerance < 0.5
                    ? DecisionType.Warning
                    : DecisionType.Info,
                Source = "Stress"
            });

            return signals;
        }

        private static DecisionType GetStressType(double level)
        {
            if (level > 0.8) return DecisionType.Critical;
            if (level > 0.6) return DecisionType.Warning;
            return DecisionType.Info;
        }

        private static double TrendScore(StressTrend trend)
        {
            return trend switch
            {
                StressTrend.Rising => 1,
                StressTrend.Stable => 0.5,
                StressTrend.Falling => 0,
                _ => 0.5
            };
        }
    }
}
