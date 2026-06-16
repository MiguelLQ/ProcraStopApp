using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Signals;

namespace ProcraStop.Core.Decisions.Rules.Behavior
{
    public class FakeProductivityRule : IDecisionRule
    {
        public int Priority => 85;
        public DecisionCategory Category => DecisionCategory.Behavior;

        public bool IsMatch(DecisionContext ctx)
        {
            var t = ctx.Tasks as TaskSignals;
            var h = ctx.Habits;

            if (t is null || h is null) return false;

            return
                t.QuickTasksCount >= 3
                && h.ConsistencyScore < 0.6
                && h.WeeklyCompletionRate <= h.ConsistencyScore
                && h.FailureRate >= 0.2;
        }

        public DecisionResult CreateDecision(DecisionContext ctx, IReadOnlyList<UnifiedSignal> signals)
        {
            var t = (TaskSignals)ctx.Tasks!;
            var h = ctx.Habits!;

            return new DecisionResult
            {
                Message = "Estás ocupado, pero no avanzando en lo importante.",
                Type = DecisionType.Warning,
                Category = Category,
                Score = 0.7,
                Reason = $"Alta actividad en tareas rápidas ({t.QuickTasksCount}) pero bajo progreso real ({h.ConsistencyScore * 100:0}%) y fallos presentes ({h.FailureRate * 100:0}%).",

                SuggestedAction = "Reduce tareas pequeñas y prioriza una tarea de impacto real hoy.",

                Insights = new List<string>
            {
                $"Quick tasks: {t.QuickTasksCount}",
                $"Consistencia: {h.ConsistencyScore * 100:0}%",
                $"Weekly: {h.WeeklyCompletionRate * 100:0}%",
                $"Fallos: {h.FailureRate * 100:0}%"
            }
            };
        }
    }
}