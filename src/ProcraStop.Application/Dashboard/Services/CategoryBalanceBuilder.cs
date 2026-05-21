

using ProcraStop.Application.Dashboard.Models;
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;


namespace ProcraStop.Application.Dashboard.Services
{
    public class CategoryBalanceBuilder
    {
        public CategoryBalanceReadModel Build(DecisionContext context)
        {
            var performance = NormalizePerformance(context);
            var behavior = context.Habits.ConsistencyScore;
            var stress = context.Stress.CurrentLevel;

            var values = new Dictionary<DecisionCategory, double>
        {
            { DecisionCategory.Performance, performance },
            { DecisionCategory.Behavior, behavior },
            { DecisionCategory.Stress, stress }
        };

            var dominant = values
                .OrderByDescending(x => x.Value)
                .First().Key;

            var imbalance = CalculateImbalance(values.Values.ToList());

            return new CategoryBalanceReadModel
            {
                Performance = performance,
                Behavior = behavior,
                Stress = stress,
                DominantCategory = dominant,
                ImbalanceScore = imbalance
            };
        }

        private double NormalizePerformance(DecisionContext ctx)
        {
            var effort = ctx.Tasks.TotalEffort;
            return Math.Min(1.0, effort / 300.0);
        }

        private double CalculateImbalance(List<double> values)
        {
            var avg = values.Average();
            var variance = values.Sum(v => Math.Pow(v - avg, 2)) / values.Count;

            return Math.Min(1.0, variance * 3); // escalado simple
        }
    }
}
