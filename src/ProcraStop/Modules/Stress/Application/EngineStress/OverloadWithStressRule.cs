using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;

namespace ProcraStop.Modules.Stress.Application.EngineStress
{
    internal class OverloadWithStressRule : IDecisionRule
    {
        public int Priority => 100; //  más alta del módulo

        public bool IsMatch(DecisionContext context)
        {
            return context.Tasks.OverdueCount > 3 &&
                   context.Stress.CurrentLevel > 0.7;
        }

        public DecisionResult CreateDecision(DecisionContext context)
        {
            return new DecisionResult
            {
                Message = "Sobrecarga crítica detectada",
                Type = DecisionType.Critical, // 
                Reason = "Tienes tareas atrasadas y un nivel alto de estrés",
                SuggestedAction = "Reduce carga inmediatamente. No añadas nuevas tareas hoy",
                Insights = new List<string>
                {
                    $"Overdue: {context.Tasks.OverdueCount}",
                    $"Stress: {context.Stress.CurrentLevel:F2}"
                }
            };
        }
    }
}
