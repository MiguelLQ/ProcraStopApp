
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
                .OrderByDescending(r => r.Priority)
                .ToList();

            if (!matchedRules.Any())
            {
                return DefaultDecision();
            }

            // 2. REGLA PRINCIPAL
            var primaryRule = matchedRules.First();
            var primary = primaryRule.CreateDecision(context);

            // 3. REGLAS DE SOPORTE
            var supportRules = matchedRules
                .Skip(1)
                .Where(r => IsCompatible(primaryRule, r))
                .Take(2)
                .ToList();

            var taskInsights = new List<string>();
            var habitInsights = new List<string>();
            var secondaryInsights = new List<string>();

            foreach (var rule in supportRules)
            {
                var result = rule.CreateDecision(context);

                CategorizeInsight(rule, result.Message,
                    taskInsights,
                    habitInsights,
                    secondaryInsights);
            }

            // 4. CONSTRUIR RESULTADO
            primary.TaskInsights = taskInsights;
            primary.HabitInsights = habitInsights;
            primary.SecondaryInsights = secondaryInsights;

            // 5. ACTUALIZAR MEMORIA - NUEVO)
            UpdateMemory(memory, primaryRule, primary);
            _memoryStore.Save(memory);

            return primary;
        }

        // ============================
        // MEMORY LOGIC
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
            IDecisionRule rule,
            DecisionResult result)
        {
            var name = rule.GetType().Name;

            memory.LastRuleName = name;
            memory.LastDecisionType = result.Type;
            memory.LastDecisionTime = DateTime.UtcNow;

            memory.RuleCooldowns[name] = DateTime.UtcNow;

            if (result.Type == DecisionType.Critical)
                memory.BurnoutAlertsLast24h++;
        }

        // ============================
        // EXISTENTE
        // ============================

        private static bool IsCompatible(IDecisionRule primary, IDecisionRule secondary)
        {
            var p = primary.GetType().Namespace ?? "";
            var s = secondary.GetType().Namespace ?? "";

            return p == s || s.Contains("Core");
        }

        private static void CategorizeInsight(
            IDecisionRule rule,
            string message,
            List<string> taskInsights,
            List<string> habitInsights,
            List<string> secondaryInsights)
        {
            var ns = rule.GetType().Namespace ?? "";

            if (ns.Contains("Tasks"))
                taskInsights.Add(message);
            else if (ns.Contains("Habits"))
                habitInsights.Add(message);
            else
                secondaryInsights.Add(message);
        }

        private static DecisionResult DefaultDecision()
        {
            return new DecisionResult
            {
                Message = "Organiza tu día y enfócate en lo importante.",
                Type = DecisionType.Info
            };
        }
    }
}
