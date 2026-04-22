using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;

namespace ProcraStop.Modules.Habits.Application.EngineHabits.Rules
{
    public class LowConsistencyRule : IDecisionRule
    {
        public int Priority => 90;

        public bool IsMatch(DecisionContext ctx)
        {
            var habits = ctx.Habits;

            if (habits is null) return false;

            return habits.ConsistencyScore < 0.5;
        }

        public DecisionResult CreateDecision(DecisionContext ctx)
        {
            var h = ctx.Habits!;

            return new DecisionResult
            {
                Message = "No estás siendo consistente.",
                Type = DecisionType.Warning,
                Reason = $"Tu consistencia actual es {h.ConsistencyScore * 100:0}%.",
                SuggestedAction = "Reduce la carga y enfócate en una rutina simple.",
                Insights = new List<string>
                {
                    $"Streak actual: {h.CurrentStreak} días",
                    $"Cumplimiento semanal: {h.WeeklyCompletionRate * 100:0}%"
                }
            };
        }
    }
}
