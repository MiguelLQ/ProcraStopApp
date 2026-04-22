using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;

namespace ProcraStop.Modules.Habits.Application.EngineHabits.Rules
{
    public class HighDisciplineRule : IDecisionRule
    {
        public int Priority => 40;

        public bool IsMatch(DecisionContext ctx)
        {
            var h = ctx.Habits;

            if (h is null) return false;

            return h.ConsistencyScore >= 0.8 && h.CurrentStreak >= 3;
        }

        public DecisionResult CreateDecision(DecisionContext ctx)
        {
            var h = ctx.Habits!;

            return new DecisionResult
            {
                Message = "Estás siendo muy consistente. Buen trabajo.",
                Type = DecisionType.Success,
                Reason = $"Mantienes una consistencia del {h.ConsistencyScore * 100:0}% con un streak de {h.CurrentStreak} días.",
                SuggestedAction = "Aprovecha este momento para avanzar en tareas importantes.",
                Insights = new List<string>
                {
                    $"Streak: {h.CurrentStreak} días",
                    $"Consistencia: {h.ConsistencyScore * 100:0}%"
                }
            };
        }
    }
}
