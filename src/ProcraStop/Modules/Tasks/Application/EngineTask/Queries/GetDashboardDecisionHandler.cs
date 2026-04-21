
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Engine;
using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.EngineTask.Context;
using ProcraStop.Modules.Tasks.Application.EngineTask.DTOs;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Modules.Tasks.Application.EngineTask.Queries
{
    public class GetDashboardDecisionHandler
    {
        private readonly ITaskRepository _repository;
        private readonly DecisionContextBuilder _contextBuilder;
        private readonly DecisionEngine _engine;

        public GetDashboardDecisionHandler(
            ITaskRepository repository,
            DecisionContextBuilder contextBuilder,
            DecisionEngine engine)
        {
            _repository = repository;
            _contextBuilder = contextBuilder;
            _engine = engine;
        }

        public async Task<DashboardDecisionDto> Handle(GetDashboardDecisionQuery query)
        {
            var tasks = await _repository.GetAllAsync();
            var context = _contextBuilder.Build(tasks);

            DecisionResult result;
            Console.WriteLine(context.TaskSignals == null);
            Console.WriteLine(context.TaskSignals?.GetType().Name);
            if (((TaskSignals)context.TaskSignals!).NoPendingTasks)
            {
                result = new DecisionResult
                {
                    Message = "No tienes tareas pendientes. Buen trabajo.",
                    Type = DecisionType.Success
                };
            }
            else
            {
                result = _engine.Evaluate(context);
            }

            return new DashboardDecisionDto
            {
                Message = result.Message,
                Type = Map(result.Type),
                SecondaryInsights = result.SecondaryInsights,
                SuggestedAction = result.SuggestedAction,
                Reason = result.Reason,
                Insights = result.Insights
            };
        }
        private GuidanceType Map(DecisionType type)
        {
            return type switch
            {
                DecisionType.Info => GuidanceType.Info,
                DecisionType.Warning => GuidanceType.Warning,
                DecisionType.Success => GuidanceType.Success,
                DecisionType.Critical => GuidanceType.Danger, // ajusta según tu enum real
                _ => GuidanceType.Info
            };
        }
    }
}
