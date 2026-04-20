
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

            var matchedRules = _rules
                .Where(r => r.IsMatch(context))
                .OrderByDescending(r => r.Priority)
                .ToList();

            var selected = matchedRules.FirstOrDefault();

            if (selected != null)
            {
                return selected.CreateDecision(context);
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
