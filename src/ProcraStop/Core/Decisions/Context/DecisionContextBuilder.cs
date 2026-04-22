using ProcraStop.Modules.Habits.Application.DTOs;
using ProcraStop.Modules.Habits.Domain.Entities;
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
            // 1. Obtener datos de stress
            var history = await _stressRepository.GetRecentAsync(7);
            var latest = history.FirstOrDefault();

            // 2. Calcular signals
            var stressSignals = _stressService.Calculate(
                taskSignals,
                habitSignals ?? HabitSignals.Empty(),
                latest,
                history
            );

            // 3. Construir contexto
            return new DecisionContext
            {
                Tasks = taskSignals,
                Habits = habitSignals ?? HabitSignals.Empty(),
                Stress = stressSignals ?? StressSignals.Empty()
            };
        }
    }
}
