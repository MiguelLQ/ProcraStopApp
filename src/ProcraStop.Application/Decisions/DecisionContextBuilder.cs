using ProcraStop.Application.Stress.Abstractions;
using ProcraStop.Application.Stress.Services;
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.Mappers;
using ProcraStop.Core.Decisions.Signals;

namespace ProcraStop.Application.Decisions
{
    public class DecisionContextBuilder
    {
        private readonly StressMetricsService _stressService;

        public DecisionContextBuilder(StressMetricsService stressService)
        {
            _stressService = stressService;
        }

        public DecisionContext Build(
            TaskSignals taskSignals,
            HabitSignals habitSignals,
            IReadOnlyList<global::ProcraStop.Domain.Stress.Entities.StressRecord> history)
        {
            var latest = history.FirstOrDefault();

            var stressSignals = _stressService.Calculate(
                taskSignals,
                habitSignals,
                latest,
                history
            ) ?? StressSignals.Empty();

            var unifiedSignals = new List<UnifiedSignal>();

            unifiedSignals.AddRange(TaskSignalExtractor.Extract(taskSignals));
            unifiedSignals.AddRange(HabitSignalExtractor.Extract(habitSignals));
            unifiedSignals.AddRange(StressSignalExtractor.Extract(stressSignals));

            return new DecisionContext(
                 taskSignals,
                 habitSignals,
                 stressSignals,
                 unifiedSignals
             );
        }
    }
    
}
