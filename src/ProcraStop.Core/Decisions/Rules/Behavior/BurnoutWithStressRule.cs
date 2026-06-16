
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Signals;
namespace ProcraStop.Core.Decisions.Rules.Behavior
{
    public class BurnoutWithStressRule : IDecisionRule
    {
        public int Priority => 110; //  top crítica
        public DecisionCategory Category => DecisionCategory.Risk;
        public bool IsMatch(DecisionContext ctx)
        {
            var t = ctx.Tasks as TaskSignals;
            var h = ctx.Habits;
            var s = ctx.Stress;

            if (t is null || h is null || s is null) return false;

            return
                (
                    t.TotalEffort >= 240
                    && h.FailureRate >= 0.3
                    && s.CurrentLevel >= 0.7
                )
                ||
                (
                    h.FailureStreak >= 3
                    && s.CurrentLevel >= 0.65
                )
                ||
                (
                    t.TotalEffort >= 200
                    && h.ConsistencyScore < 0.5
                    && h.WeeklyCompletionRate <= h.ConsistencyScore
                    && s.CurrentLevel >= 0.7
                );
        }

        public DecisionResult CreateDecision(DecisionContext ctx, IReadOnlyList<UnifiedSignal> signals)
        {
            var t = (TaskSignals)ctx.Tasks!;
            var h = ctx.Habits!;
            var s = ctx.Stress!;

            return new DecisionResult
            {
                Message = "Riesgo crítico de burnout detectado",
                Type = DecisionType.Critical,
                Category = Category,
                Score = 1.0,
                Reason = BuildReason(t, h, s),

                SuggestedAction = "Reduce la carga inmediatamente. Prioriza descanso real o tareas mínimas.",

                Insights = new List<string>
                {
                    $"Carga: {t.TotalEffort} min",
                    $"Consistencia: {h.ConsistencyScore * 100:0}%",
                    $"Weekly: {h.WeeklyCompletionRate * 100:0}%",
                    $"Fallos: {h.FailureRate * 100:0}%",
                    $"Racha de fallos: {h.FailureStreak}",
                    $"Stress: {s.CurrentLevel * 100:0}%"
                }
            };
        }

        private static string BuildReason(TaskSignals t, HabitSignals h, StressSignals s)
        {
            if (h.FailureStreak >= 3)
                return $"Acumulas {h.FailureStreak} días consecutivos fallando bajo estrés elevado.";

            if (h.FailureRate >= 0.3 && t.TotalEffort >= 240)
                return $"Alta carga ({t.TotalEffort} min) combinada con fallos ({h.FailureRate * 100:0}%).";

            return $"Bajo rendimiento reciente con estrés elevado ({s.CurrentLevel * 100:0}%).";
        }
    }
}
