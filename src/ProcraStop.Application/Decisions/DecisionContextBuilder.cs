using ProcraStop.Application.Stress.Abstractions;
using ProcraStop.Application.Stress.Services;
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.Mappers;
using ProcraStop.Core.Decisions.Signals;

namespace ProcraStop.Application.Decisions
{
    public class DecisionContextBuilder
    {
        private readonly IStressRepository _stressRepository;
        private readonly StressMetricsService _stressService;

        public DecisionContextBuilder(
            IStressRepository stressRepository,
            StressMetricsService stressService)
        {
            _stressRepository = stressRepository;
            _stressService = stressService;
        }

        public async Task<DecisionContext> BuildAsync(
    TaskSignals taskSignals,
    HabitSignals habitSignals)
        {
            var history = await _stressRepository.GetRecentAsync(7);
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
