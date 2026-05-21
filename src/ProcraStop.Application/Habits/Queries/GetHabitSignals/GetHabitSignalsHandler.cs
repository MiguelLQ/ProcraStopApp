

using ProcraStop.Application.Habits.Abstractions;
using ProcraStop.Application.Habits.DTOs;
using ProcraStop.Application.Habits.Interfaces;

namespace ProcraStop.Application.Habits.Queries.GetHabitSignals
{
    public class GetHabitSignalsHandler
    {
        private readonly IHabitRepository _habitRepository;
        private readonly IHabitLogRepository _logRepository;
        private readonly IHabitMetricsService _metricsService;

        public GetHabitSignalsHandler(
            IHabitRepository habitRepository,
            IHabitLogRepository logRepository,
            IHabitMetricsService metricsService)
        {
            _habitRepository = habitRepository;
            _logRepository = logRepository;
            _metricsService = metricsService;
        }

        public async Task<HabitSignals> Handle(GetHabitSignalsQuery query)
        {
            var habits = await _habitRepository.GetAllAsync();
            var logs = await _logRepository.GetAllAsync();

            return _metricsService.Calculate(habits, logs, DateTime.UtcNow);
        }
    }
}
