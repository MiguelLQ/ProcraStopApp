

using Microsoft.EntityFrameworkCore.Migrations;
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.Engine;
using ProcraStop.Modules.Dashboard.Application.Abstractions;
using ProcraStop.Modules.Dashboard.Application.Core;
using ProcraStop.Modules.Dashboard.Application.Models;
using ProcraStop.Modules.Dashboard.Application.Models.Historial;
using ProcraStop.Modules.Habits.Application.Abstractions;
using ProcraStop.Modules.Habits.Application.Interfaces;
using ProcraStop.Modules.Stress.Application.Services;
using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;
using ProcraStop.Modules.Tasks.Infraestructure.Services;

namespace ProcraStop.Modules.Dashboard.Application.Services
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
            // 1. DATA
            var tasks = await _taskReadService.GetDashboardDataAsync(cancellationToken);
            var habits = await _habitRepo.GetAllAsync();
                var logs = await _habitLogRepo.GetAllAsync();

            var now = DateTime.UtcNow;
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

            await _historyService.SaveAsync(new SystemSnapshot
            {
                Timestamp = DateTime.UtcNow,
                Stress = context.Stress.CurrentLevel,
                Performance = NormalizePerformance(context),
                Behavior = context.Habits.ConsistencyScore
            });
            // 5. historial (ultimos 7 dias)
            var history = await _historyService.GetLastDays(7);
            // 5. TENDENCIA
            var trend = _trendEngine.Build(history);


            var signals = _signalProjection.Build(context, decision);

            // . PROJECTION (mapping limpio a READ MODEL)
            Console.WriteLine($"Matched signals: {context.Signals.Count}");
            Console.WriteLine($"Decision Type: {decision.Type}");
            Console.WriteLine($"TaskInsights: {decision.TaskInsights?.Count}");
            Console.WriteLine($"HabitInsights: {decision.HabitInsights?.Count}");
            Console.WriteLine($"SecondaryInsights: {decision.SecondaryInsights?.Count}");

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
    }
}
