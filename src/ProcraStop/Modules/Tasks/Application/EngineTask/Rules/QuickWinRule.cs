using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Core.Decisions.Signals;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Modules.Tasks.Application.EngineTask.Rules
{
    public class QuickWinRule : IDecisionRule
    {
        public int Priority => 50;
        public DecisionCategory Category => DecisionCategory.Recovery;
        public bool IsMatch(DecisionContext ctx)
        {
            var t = (TaskSignals)ctx.Tasks!;

            return t.OverdueCount >= 1 && t.QuickTasksCount >= 2;
        }

        public DecisionResult CreateDecision(DecisionContext ctx, IReadOnlyList<UnifiedSignal> signals)
        {
            var t = (TaskSignals)ctx.Tasks!;

            return new DecisionResult
            {
                Message = "Tienes tareas atrasadas pero fáciles.",
                Type = DecisionType.Info,
                Category = Category,
                Score = 0.65,
                Reason = $"Tienes {t.OverdueCount} tareas atrasadas y {t.QuickTasksCount} tareas rápidas disponibles.",
                SuggestedAction = "Haz una tarea rápida ahora para recuperar control.",
                Insights = new List<string>
                {
                    $"Atrasadas: {t.OverdueCount}",
                    $"Rápidas: {t.QuickTasksCount}"
                }
            };
        }
    }
}
