
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Memory;
using ProcraStop.Core.Decisions.Rules;

namespace ProcraStop.Core.Decisions.Engine
{
    public sealed class DecisionEngine
    {
        private readonly IEnumerable<IDecisionRule> _rules;
        private readonly IDecisionMemoryStore _memoryStore;

        public DecisionEngine(
            IEnumerable<IDecisionRule> rules,
            IDecisionMemoryStore memoryStore)
        {
            _rules = rules;
            _memoryStore = memoryStore;
        }

        public DecisionResult Evaluate(DecisionContext context)
        {
            var memory = _memoryStore.Get();

            // 1. FILTRAR REGLAS
            var matchedRules = _rules
                .Where(r => r.IsMatch(context))
                .Where(r => !IsInCooldown(r, memory))
                .ToList();

            if (!matchedRules.Any())
            {
                return DefaultDecision(context);
            }

            // 2. EVALUAR TODAS (SCORING)
            var evaluated = matchedRules
                .Select(r =>
                {
                    var result = r.CreateDecision(context, context.Signals);
                    return result;
                })
                .ToList();

            // 3. AGRUPAR POR CATEGORÍA
            var grouped = evaluated
                .GroupBy(r => r.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    TotalScore = g.Sum(x => x.Score),
                    Top = g.OrderByDescending(x => x.Score).First()
                })
                .OrderByDescending(x => x.TotalScore)
                .ToList();

            // 4. SELECCIÓN PRINCIPAL
            var winnerGroup = grouped.First();
            var primary = winnerGroup.Top;

            // 5. SOPORTE
            var support = grouped
                .Skip(1)
                .Where(g => IsCategoryCompatible(winnerGroup.Category, g.Category))
                .Take(2)
                .Select(g => g.Top)
                .ToList();

            var taskInsights = new List<string>();
            var habitInsights = new List<string>();
            var secondaryInsights = new List<string>();

            foreach (var s in support)
            {
                CategorizeInsightByCategory(
                    s.Category,
                    s.Message,
                    taskInsights,
                    habitInsights,
                    secondaryInsights);
            }

            // 6. CONSTRUIR RESULTADO
            primary.TaskInsights = taskInsights;
            primary.HabitInsights = habitInsights;
            primary.SecondaryInsights = secondaryInsights;

            // 7. MEMORIA
            UpdateMemory(memory, primary);
            _memoryStore.Save(memory);

            return primary;
        }

        // ============================
        // MEMORY
        // ============================

        private static bool IsInCooldown(IDecisionRule rule, DecisionMemory memory)
        {
            var name = rule.GetType().Name;

            if (!memory.RuleCooldowns.TryGetValue(name, out var lastTime))
                return false;

            return (DateTime.UtcNow - lastTime).TotalMinutes < 30;
        }
        // TODO: Mejorar esta lógica para manejar diferentes tipos de cooldowns
        // y evitar penalizar reglas compatibles que se activan juntas.
        private static void UpdateMemory(
             DecisionMemory memory,
             DecisionResult result)
        {
            memory.LastDecisionType = result.Type;
            memory.LastDecisionTime = DateTime.UtcNow;

            if (result.Type == DecisionType.Critical)
                memory.BurnoutAlertsLast24h++;
        }
        // ============================
        // COMPATIBILIDAD
        // ============================

        private static bool IsCategoryCompatible(
    DecisionCategory primary,
    DecisionCategory secondary)
        {
            if (primary == secondary) return true;

            // permitir mezcla controlada
            if (primary == DecisionCategory.Performance && secondary == DecisionCategory.Behavior)
                return true;

            if (primary == DecisionCategory.Risk)
                return true;

            return false;
        }
        // ============================
        // INSIGHTS
        // ============================

        private static void CategorizeInsightByCategory(
            DecisionCategory category,
            string message,
            List<string> taskInsights,
            List<string> habitInsights,
            List<string> secondaryInsights)
        {
            switch (category)
            {
                case DecisionCategory.Performance:
                    taskInsights.Add(message);
                    break;

                case DecisionCategory.Behavior:
                case DecisionCategory.Recovery:
                    habitInsights.Add(message);
                    break;

                default:
                    secondaryInsights.Add(message);
                    break;
            }
        }

        // ============================
        // DEFAULT
        // ============================

        private static DecisionResult DefaultDecision(DecisionContext ctx)
        {
            if (ctx.Stress is not null && ctx.Stress.CurrentLevel > 0.6)
            {
                return new DecisionResult
                {
                    Message = "Reduce la carga y enfócate en lo esencial.",
                    Type = DecisionType.Warning,
                    Score = 0.5,
                    Category = DecisionCategory.Stress
                };
            }

            return new DecisionResult
            {
                Message = "Mantén el enfoque en tus prioridades.",
                Type = DecisionType.Info,
                Score = 0.3,
                Category = DecisionCategory.Performance
            };
        }
    }
}
