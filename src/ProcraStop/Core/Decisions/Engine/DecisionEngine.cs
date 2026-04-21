
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;

namespace ProcraStop.Core.Decisions.Engine
{
    public sealed class DecisionEngine
    {
        private readonly IEnumerable<IDecisionRule> _rules;

        public DecisionEngine(IEnumerable<IDecisionRule> rules)
        {
            _rules = rules;
        }

        public DecisionResult Evaluate(DecisionContext context)
        {
            var matchedRules = _rules
                .Where(r => r.IsMatch(context))
                .OrderByDescending(r => r.Priority)
                .ToList();

            var selected = matchedRules.FirstOrDefault();
            var secondary = matchedRules.Skip(1).Take(2).ToList();

            if (selected is null)
            {
                return new DecisionResult
                {
                    Message = "Organiza tu día y enfócate en lo importante.",
                    Type = DecisionType.Info
                };
            }

            var decision = selected.CreateDecision(context);

            decision.SecondaryInsights = secondary
                .Select(r => r.CreateDecision(context).Message)
                .ToList();

            return decision;
        }
    }
}
