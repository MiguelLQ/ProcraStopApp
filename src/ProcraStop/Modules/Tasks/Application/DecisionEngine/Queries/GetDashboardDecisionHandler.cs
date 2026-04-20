
using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.DTOs;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.Rules;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.DecisionEngine.Queries
{
    public class GetDashboardDecisionHandler
    {
        private readonly ITaskRepository _repository;
        private readonly IEnumerable<IDecisionRule> _rules;
        public GetDashboardDecisionHandler(
          ITaskRepository repository,
          IEnumerable<IDecisionRule> rules)
            {
                _repository = repository;
                _rules = rules;
            }

        public async Task<DashboardDecisionDto> Handle(GetDashboardDecisionQuery query)
        {
            var tasks = await _repository.GetAllAsync();

            var pending = tasks.Where(t => !t.IsCompleted && !t.IsArchived).ToList();

            // 🟢 caso especial: sin tareas
            if (!pending.Any())
            {
                return new DashboardDecisionDto
                {
                    Message = "No tienes tareas pendientes. Buen trabajo.",
                    Type = GuidanceType.Success
                };
            }

            // 🔥 evaluar reglas
            var matchedRules = _rules
                .Where(r => r.IsMatch(tasks))
                .OrderByDescending(r => r.Priority)
                .ToList();

            var selectedRule = matchedRules.FirstOrDefault();

            if (selectedRule != null)
            {
                return selectedRule.CreateDecision(tasks);
            }

            // 🔵 default
            return new DashboardDecisionDto
            {
                Message = "Organiza tu día y enfócate en lo importante.",
                Type = GuidanceType.Info
            };
        }
    }
}
