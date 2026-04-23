

using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;

namespace ProcraStop.Core.Decisions.Rules.Behavior
{
    public class FakeProductivityWithStressRule : IDecisionRule
    {
        public int Priority => 95; // mayor que la original

        public bool IsMatch(DecisionContext ctx)
        {
            var t = ctx.Tasks!;
            var h = ctx.Habits;
            var s = ctx.Stress;

            if (h is null) return false;

            return t.QuickTasksCount >= 3 &&
                   h.ConsistencyScore < 0.5 &&
                   s.CurrentLevel > 0.6;
        }

        public DecisionResult CreateDecision(DecisionContext ctx)
        {
            var t = ctx.Tasks!;
            var h = ctx.Habits!;
            var s = ctx.Stress;

            return new DecisionResult
            {
                Message = "Estás ocupado y además bajo presión",
                Type = DecisionType.Warning,
                Reason = "Productividad superficial combinada con estrés elevado",
                SuggestedAction = "Detente y enfócate en una sola tarea importante",
                Insights = new List<string>
                {
                    $"QuickTasks: {t.QuickTasksCount}",
                    $"Consistencia: {h.ConsistencyScore * 100:0}%",
                    $"Stress: {s.CurrentLevel:F2}"
                }
            };
        }
    }
}
