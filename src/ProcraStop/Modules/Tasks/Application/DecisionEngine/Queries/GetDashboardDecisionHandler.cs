
using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.DTOs;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.Rules;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.Signals;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.DecisionEngine.Queries
{
    public class GetDashboardDecisionHandler
    {
        private readonly ITaskRepository _repository;
        private readonly IEnumerable<IDecisionRule> _rules;
        private readonly TaskSignalsBuilder _signalsBuilder;
        public GetDashboardDecisionHandler(
          ITaskRepository repository,
          IEnumerable<IDecisionRule> rules,
          TaskSignalsBuilder signalsBuilder)
            {
                _repository = repository;
                _rules = rules;
                _signalsBuilder = signalsBuilder;
            }

        public async Task<DashboardDecisionDto> Handle(GetDashboardDecisionQuery query)
        {
            var tasks = await _repository.GetAllAsync();
            var signals = _signalsBuilder.Build(tasks);

            // caso especial
            if (signals.NoPendingTasks)
            {
                return new DashboardDecisionDto
                {
                    Message = "No tienes tareas pendientes. Buen trabajo.",
                    Type = GuidanceType.Success
                };
            }

            //evaluar reglas
            var matchedRules = _rules
                .Where(r => r.IsMatch(signals))
                .OrderByDescending(r => r.Priority)
                .ToList();

            var selected = matchedRules.FirstOrDefault();

            if (selected != null)
            {
                return selected.CreateDecision(signals);
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
