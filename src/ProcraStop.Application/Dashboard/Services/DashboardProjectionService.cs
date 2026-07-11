

using ProcraStop.Application.Dashboard.Abstractions;
using ProcraStop.Application.Dashboard.Core;
using ProcraStop.Application.Dashboard.Interfaces;
using ProcraStop.Application.Dashboard.Models;
using ProcraStop.Application.Dashboard.Models.Historial;
using ProcraStop.Application.Decisions;
using ProcraStop.Application.Habits.Abstractions;
using ProcraStop.Application.Habits.Interfaces;
using ProcraStop.Application.Stress.Services;
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.EngineTask.Signals;
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.Engine;


namespace ProcraStop.Application.Dashboard.Services
{
    public class DashboardProjectionService : IDashboardProjectionService
    {
        private readonly ITaskReadService _taskReadService;
        private readonly IHabitRepository _habitRepo;
        private readonly IHabitLogRepository _habitLogRepo;

        private readonly TaskSignalsBuilder _taskSignals;
        private readonly IHabitMetricsService _habitMetrics;

        private readonly DecisionContextBuilder _contextBuilder;
        private readonly DecisionEngine _engine;

        private readonly StressEventDetector _stressDetector;
        private readonly ISignalProjectionService _signalProjection;

        private readonly ISystemSnapshotRepository _historyService;
        private readonly TrendEngine _trendEngine;

        private readonly CategoryBalanceBuilder _categoryBalanceBuilder;
        private readonly SystemStateBuilder _systemStateBuilder;
        public DashboardProjectionService(
            ITaskReadService taskRepo,
            IHabitRepository habitRepo,
            IHabitLogRepository habitLogRepo,
            TaskSignalsBuilder taskSignals,
            IHabitMetricsService habitMetrics,
            DecisionContextBuilder contextBuilder,
            DecisionEngine engine,
            StressEventDetector stressDetector,
            ISignalProjectionService signalProjection,
            ISystemSnapshotRepository historyService,
            TrendEngine trendEngine,
            CategoryBalanceBuilder categoryBalanceBuilder,
            SystemStateBuilder systemStateBuilder)
        {
            _taskReadService = taskRepo;
            _habitRepo = habitRepo;
            _habitLogRepo = habitLogRepo;
            _taskSignals = taskSignals;
            _habitMetrics = habitMetrics;
            _contextBuilder = contextBuilder;
            _engine = engine;
            _stressDetector = stressDetector;
            _signalProjection = signalProjection;
            _historyService = historyService;
            _trendEngine = trendEngine;
            _categoryBalanceBuilder = categoryBalanceBuilder;
            _systemStateBuilder = systemStateBuilder;
        }

            public async Task<DashboardReadModel> BuildAsync( CancellationToken cancellationToken= default)
            {
            var now = DateTime.UtcNow;
            var historyWindowStart = now.Date.AddDays(-6);
            // 1. DATA
            var tasksTask = _taskReadService.GetDashboardDataAsync(cancellationToken);
            var habitsTask = _habitRepo.GetActiveAsync(cancellationToken);
            var logsTask = _habitLogRepo.GetRangeAsync(historyWindowStart, now.Date, cancellationToken);

            await Task.WhenAll(tasksTask, habitsTask, logsTask);

            var tasks = await tasksTask;
            var habits = await habitsTask;
            var logs = await logsTask;
            // 2. SIGNALS
            var taskSignals = _taskSignals.Build(tasks);
            var habitSignals = _habitMetrics.Calculate(habits, logs, now);
            // 3. context
            var context = await _contextBuilder.BuildAsync(taskSignals, habitSignals);

            var balance = _categoryBalanceBuilder.Build(context);

            var systemState = _systemStateBuilder.Build(context);
            // 4. DECISION
            var decision = _engine.Evaluate(context);
            //  guardar estado del sistema
            await _stressDetector.EvaluateAndRegisterAsync(taskSignals, habitSignals);

            var snapshot = new SystemSnapshot
            {
                Timestamp = now,
                Stress = context.Stress.CurrentLevel,
                Performance = NormalizePerformance(context),
                Behavior = context.Habits.ConsistencyScore
            };
            // 5. historial (ultimos 7 dias)
            var history = await _historyService.GetLastDays(7);
            if (ShouldSaveSnapshot(history.LastOrDefault(), snapshot))
            {
                await _historyService.SaveAsync(snapshot);
                history.Add(snapshot);
            }
            // 5. TENDENCIA
            var trend = _trendEngine.Build(history);


            var signals = _signalProjection.Build(context, decision);

            return new DashboardReadModel
            {
                CoreState = new CoreStateReadModel
                {
                    Message = decision.Message,
                    Type = decision.Type,
                    Reason = decision.Reason,
                    SuggestedAction = decision.SuggestedAction
                },

                Signals = signals,

                TaskInsights = decision.TaskInsights ?? new(),
                HabitInsights = decision.HabitInsights ?? new(),
                StressInsights = decision.SecondaryInsights ?? new(),
                Trend = trend,
                CategoryBalance = new List<CategoryBalanceReadModel> { balance },
                SystemState = systemState,
            };
        }
        private double NormalizePerformance(DecisionContext ctx)
        {
            var effort = ctx.Tasks.TotalEffort;

            return Math.Min(1.0, effort / 300.0);
        }

        private static bool ShouldSaveSnapshot(SystemSnapshot? last, SystemSnapshot current)
        {
            if (last is null)
                return true;

            var minutes = (current.Timestamp - last.Timestamp).TotalMinutes;
            if (minutes >= 30)
                return true;

            var delta =
                Math.Abs(current.Stress - last.Stress) +
                Math.Abs(current.Performance - last.Performance) +
                Math.Abs(current.Behavior - last.Behavior);

            return delta >= 0.25;
        }
    }
}
