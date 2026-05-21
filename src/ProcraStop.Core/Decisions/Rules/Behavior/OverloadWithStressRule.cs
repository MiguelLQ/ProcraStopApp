using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Signals;

namespace ProcraStop.Core.Decisions.Rules.Behavior
{
    internal class OverloadWithStressRule : IDecisionRule
    {
        public int Priority => 100; //  más alta del módulo
        public DecisionCategory Category => DecisionCategory.Risk;
        public bool IsMatch(DecisionContext context)
        {
            return context.Tasks.OverdueCount > 3 &&
                   context.Stress.CurrentLevel > 0.7;
        }

        public DecisionResult CreateDecision(DecisionContext ctx, IReadOnlyList<UnifiedSignal> signals)
        {
            return new DecisionResult
            {
                Message = "Sobrecarga crítica detectada",
                Type = DecisionType.Critical, // 
                Category = Category,
                Score = 0.95,
                Reason = "Tienes tareas atrasadas y un nivel alto de estrés",
                SuggestedAction = "Reduce carga inmediatamente. No añadas nuevas tareas hoy",
                Insights = new List<string>
                {
                    $"Overdue: {ctx.Tasks.OverdueCount}",
                    $"Stress: {ctx.Stress.CurrentLevel:F2}"
                }
            };
        }
    }
}
