

using ProcraStop.Application.Dashboard.Models.Historial;

namespace ProcraStop.Application.Dashboard.Core
{
    public class TrendEngine
    {
        public TrendReadModel Build(List<SystemSnapshot> history)
        {
            if (history.Count < 2)
            {
                return new TrendReadModel
                {
                    StressTrend = TrendDirection.Stable,
                    PerformanceTrend = TrendDirection.Stable,
                    BehaviorTrend = TrendDirection.Stable,
                    GlobalTrend = "Insufficient Data"
                };
            }

            var stress = CalculateTrend(history.Select(x => x.Stress));
            var performance = CalculateTrend(history.Select(x => x.Performance));
            var behavior = CalculateTrend(history.Select(x => x.Behavior));

            return new TrendReadModel
            {
                StressTrend = stress,
                PerformanceTrend = performance,
                BehaviorTrend = behavior,
                GlobalTrend = ComputeGlobal(stress, performance, behavior)
            };
        }

        private TrendDirection CalculateTrend(IEnumerable<double> values)
        {
            var list = values.ToList();

            var firstHalf = list.Take(list.Count / 2).Average();
            var secondHalf = list.Skip(list.Count / 2).Average();

            var diff = secondHalf - firstHalf;

            if (Math.Abs(diff) < 0.05)
                return TrendDirection.Stable;

            return diff > 0 ? TrendDirection.Rising : TrendDirection.Falling;
        }

        private string ComputeGlobal(
            TrendDirection stress,
            TrendDirection performance,
            TrendDirection behavior)
        {
            if (stress == TrendDirection.Rising && performance == TrendDirection.Falling)
                return "Degrading";

            if (performance == TrendDirection.Rising && stress == TrendDirection.Falling)
                return "Improving";

            return "Stable";
        }
    }
}
