using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;

namespace ProcraStop.Modules.Habits.Application.EngineHabits.Rules
{
    public class RecoveryMomentumRule : IDecisionRule
    {
        public int Priority => 70;

        public bool IsMatch(DecisionContext ctx)
        {
            var h = ctx.Habits;
            if (h is null) return false;

            return h.ConsistencyScore >= 0.5 &&
                   h.CurrentStreak >= 2;
        }

        public DecisionResult CreateDecision(DecisionContext ctx)
        {
            var h = ctx.Habits!;

            return new DecisionResult
            {
                Message = "Estás recuperando el ritmo.",
                Type = DecisionType.Info,
                Reason = $"Tu consistencia ha mejorado y llevas {h.CurrentStreak} días seguidos.",
                SuggestedAction = "Mantén la rutina, no aumentes carga aún.",
                Insights = new List<string>
            {
                $"Streak: {h.CurrentStreak} días",
                $"Consistencia: {h.ConsistencyScore * 100:0}%"
            }
            };
        }
    }
}
