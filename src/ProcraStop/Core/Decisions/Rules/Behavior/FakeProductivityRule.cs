using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Core.Decisions.Rules.Behavior
{
    public class FakeProductivityRule : IDecisionRule
    {
        public int Priority => 85;

        public bool IsMatch(DecisionContext ctx)
        {
            var t = (TaskSignals)ctx.Tasks!;
            var h = ctx.Habits;

            if (h is null) return false;

            return t.QuickTasksCount >= 3 && h.ConsistencyScore < 0.5;
        }

        public DecisionResult CreateDecision(DecisionContext ctx)
        {
            var t = (TaskSignals)ctx.Tasks!;
            var h = ctx.Habits!;

            return new DecisionResult
            {
                Message = "Estás ocupado, pero no avanzando en lo importante.",
                Type = DecisionType.Warning,
                Reason = $"Muchas tareas rápidas ({t.QuickTasksCount}) pero baja consistencia ({h.ConsistencyScore * 100:0}%).",
                SuggestedAction = "Elige una tarea importante y avanza aunque sea poco.",
                Insights = new List<string>
                {
                    $"Tareas rápidas: {t.QuickTasksCount}",
                    $"Consistencia: {h.ConsistencyScore * 100:0}%"
                }
            };
        }
    }
}
