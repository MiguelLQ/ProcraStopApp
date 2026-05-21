using ProcraStop.Core.Decisions.Mappers;
using ProcraStop.Core.Decisions.Signals;
using ProcraStop.Modules.Habits.Application.DTOs;
using ProcraStop.Modules.Stress.Application.Abstractions;
using ProcraStop.Modules.Stress.Application.DTOs;
using ProcraStop.Modules.Stress.Application.Services;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Core.Decisions.Context
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
            var safeHabits = habitSignals ?? HabitSignals.Empty();

            // 1. STRESS COMPUTATION
            var history = await _stressRepository.GetRecentAsync(7);
            var latest = history.FirstOrDefault();

            var stressSignals = _stressService.Calculate(
                taskSignals,
                safeHabits,
                latest,
                history
            ) ?? StressSignals.Empty();

            // 2.  SIGNAL EXTRACTION LAYER (CORRECTO)
            var unifiedSignals = new List<UnifiedSignal>();

            unifiedSignals.AddRange(TaskSignalExtractor.Extract(taskSignals));
            unifiedSignals.AddRange(HabitSignalExtractor.Extract(safeHabits));
            unifiedSignals.AddRange(StressSignalExtractor.Extract(stressSignals));

            // 3. CONTEXT FINAL
            return new DecisionContext
            {
                Tasks = taskSignals,
                Habits = safeHabits,
                Stress = stressSignals,
                Signals = unifiedSignals
            };
        }
    }
    
}
