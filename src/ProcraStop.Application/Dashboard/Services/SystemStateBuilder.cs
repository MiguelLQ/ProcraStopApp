

using ProcraStop.Application.Dashboard.Models;
using ProcraStop.Core.Decisions.Context;

namespace ProcraStop.Application.Dashboard.Services
{
    public class SystemStateBuilder
    {
        public SystemStateReadModel Build(DecisionContext ctx)
        {
            var stress = ctx.Stress.CurrentLevel;
            var performance = NormalizePerformance(ctx);
            var behavior = ctx.Habits.ConsistencyScore;

            var overall =
                (performance * 0.4) +
                (behavior * 0.3) +
                ((1 - stress) * 0.3);

            return new SystemStateReadModel
            {
                OverallScore = Math.Round(overall, 2),
                StressPressure = Math.Round(stress, 2),
                PerformanceHealth = Math.Round(performance, 2),
                BehavioralStability = Math.Round(behavior, 2),
                Label = ResolveLabel(overall, stress)
            };
        }

        private double NormalizePerformance(DecisionContext ctx)
        {
            return Math.Min(1.0, ctx.Tasks.TotalEffort / 300.0);
        }

        private string ResolveLabel(double score, double stress)
        {
            if (stress > 0.7) return "Overloaded";
            if (score > 0.75) return "Optimal";
            if (score > 0.5) return "Stable";

            return "At Risk";
        }
    }
}
