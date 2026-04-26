

using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Signals;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Core.Decisions.Rules.Behavior
{
    public class FakeProductivityWithStressRule : IDecisionRule
    {
        public int Priority => 95; // mayor que la original
        public DecisionCategory Category => DecisionCategory.Behavior;
        public bool IsMatch(DecisionContext ctx)
        {
            var t = ctx.Tasks as TaskSignals;
            var h = ctx.Habits;
            var s = ctx.Stress;

            if (t is null || h is null || s is null) return false;

            return
                t.QuickTasksCount >= 4
                && h.ConsistencyScore < 0.6
                && h.WeeklyCompletionRate <= h.ConsistencyScore
                && h.FailureRate >= 0.2
                && s.CurrentLevel >= 0.6;
        }

        public DecisionResult CreateDecision(DecisionContext ctx, IReadOnlyList<UnifiedSignal> signals)
        {
            var t = (TaskSignals)ctx.Tasks!;
            var h = ctx.Habits!;
            var s = ctx.Stress!;

            return new DecisionResult
            {
                Message = "Estás ocupado, bajo presión y sin avanzar en lo importante.",
                Type = DecisionType.Warning,
                Category = Category,
                Score = 0.85,

                Reason = $"Alta actividad superficial ({t.QuickTasksCount} tareas rápidas) combinada con bajo progreso ({h.ConsistencyScore * 100:0}%) y estrés elevado ({s.CurrentLevel * 100:0}%).",

                SuggestedAction = "Detente. Elige una sola tarea importante y avanza sin interrupciones.",

                Insights = new List<string>
                {
                    $"QuickTasks: {t.QuickTasksCount}",
                    $"Consistencia: {h.ConsistencyScore * 100:0}%",
                    $"Weekly: {h.WeeklyCompletionRate * 100:0}%",
                    $"Fallos: {h.FailureRate * 100:0}%",
                    $"Stress: {s.CurrentLevel * 100:0}%"
                }
            };
        }
    }
}
