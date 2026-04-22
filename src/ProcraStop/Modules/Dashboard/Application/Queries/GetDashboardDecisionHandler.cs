using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.Engine;
using ProcraStop.Modules.Dashboard.Application.DTOs;
using ProcraStop.Modules.Habits.Application.Abstractions;
using ProcraStop.Modules.Habits.Application.Interfaces;
using ProcraStop.Modules.Habits.Application.Services;
using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Modules.Dashboard.Application.Queries
{
    public class GetDashboardDecisionHandler
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IHabitRepository _habitRepository;
        private readonly IHabitLogRepository _habitLogRepository;

        private readonly TaskSignalsBuilder _taskBuilder;
        private readonly IHabitMetricsService _habitMetrics;

        private readonly DecisionContextBuilder _contextBuilder;
        private readonly DecisionEngine _engine;

        public GetDashboardDecisionHandler(
            ITaskRepository taskRepository,
            IHabitRepository habitRepository,
            IHabitLogRepository habitLogRepository,
            TaskSignalsBuilder taskBuilder,
            IHabitMetricsService habitMetrics,
            DecisionContextBuilder contextBuilder,
            DecisionEngine engine)
        {
            _taskRepository = taskRepository;
            _habitRepository = habitRepository;
            _habitLogRepository = habitLogRepository;
            _taskBuilder = taskBuilder;
            _habitMetrics = habitMetrics;
            _contextBuilder = contextBuilder;
            _engine = engine;
        }

        public async Task<DashboardDecisionDto> Handle(GetDashboardDecisionQuery query)
        {
            // 1. DATA CRUDA
            var tasks = await _taskRepository.GetAllAsync();
            var habits = await _habitRepository.GetAllAsync();
            var logs = await _habitLogRepository.GetAllAsync();

            var now = DateTime.UtcNow;

            // 2. SIGNALS
            var taskSignals = _taskBuilder.Build(tasks);
            var habitSignals = _habitMetrics.Calculate(habits, logs, now);
            Console.WriteLine($"Tasks: {tasks.Count}");
            Console.WriteLine($"Habits: {habits.Count}");
            Console.WriteLine($"Logs: {logs.Count}");
            // 3. CONTEXTO
            var context = await _contextBuilder.BuildAsync(taskSignals, habitSignals);

            // 4. DECISIÓN
            var result = _engine.Evaluate(context);

            // 5. OUTPUT (SIN MAPPING)
            return new DashboardDecisionDto
            {
                Message = result.Message,
                Type = result.Type, //  DIRECTO (DecisionType)
                SecondaryInsights = result.SecondaryInsights,
                SuggestedAction = result.SuggestedAction,
                Reason = result.Reason,
                Insights = (result.TaskInsights ?? new List<string>())
                    .Concat(result.HabitInsights ?? new List<string>())
                    .ToList()
            };
        }
    }
}
