
using Android.Icu.Number;
using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.Context;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.DTOs;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.Rules;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.DecisionEngine.Queries
{
    public class GetDashboardDecisionHandler
    {
        private readonly ITaskRepository _repository;
        private readonly IEnumerable<IDecisionRule> _rules;
        private readonly DecisionContextBuilder _contextBuilder;
        public GetDashboardDecisionHandler(
          ITaskRepository repository,
          IEnumerable<IDecisionRule> rules,
          DecisionContextBuilder contextBuilder)
            {
                _repository = repository;
                _rules = rules;
                _contextBuilder = contextBuilder;
            }

        public async Task<DashboardDecisionDto> Handle(GetDashboardDecisionQuery query)
        {
            var tasks = await _repository.GetAllAsync();
            var context = _contextBuilder.Build(tasks);
       
            // caso especial
            if (context.Tasks.NoPendingTasks)
            {
                return new DashboardDecisionDto
                {
                    Message = "No tienes tareas pendientes. Buen trabajo.",
                    Type = GuidanceType.Success
                };
            }
            Console.WriteLine($"Pending: {context.Tasks.PendingCount}");
            Console.WriteLine($"Overdue: {context.Tasks.OverdueCount}");
            Console.WriteLine($"Quick: {context.Tasks.QuickTasksCount}");
            Console.WriteLine($"Heavy: {context.Tasks.HeavyTasksCount}");
            Console.WriteLine($"Effort: {context.Tasks.TotalEffort}");
            foreach (var rule in _rules)
            {
                Console.WriteLine($"{rule.GetType().Name} => {rule.IsMatch(context)}");
            }
            var matchedRules = _rules
                .Where(r => r.IsMatch(context))
                .OrderByDescending(r => r.Priority)
                .ToList();

            var selected = matchedRules.FirstOrDefault();
            var secondary = matchedRules.Skip(1).Take(2).ToList();
            if (selected != null)
            {
                var decision = selected.CreateDecision(context);

                // AQUÍ ESTÁ LA CLAVE
                decision.SecondaryInsights = secondary
                    .Select(r => r.CreateDecision(context).Message)
                    .ToList();
                return decision;

            }

            //default
            return new DashboardDecisionDto
            {
                Message = "Organiza tu día y enfócate en lo importante.",
                Type = GuidanceType.Info
            };
        }
    }
}
