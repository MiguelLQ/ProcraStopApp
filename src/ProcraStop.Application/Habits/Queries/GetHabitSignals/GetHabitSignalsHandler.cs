

using ProcraStop.Application.Habits.Abstractions;
using ProcraStop.Application.Habits.Interfaces;
using ProcraStop.Core.Decisions.Signals;

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

        public async Task<HabitSignals> Handle(
            GetHabitSignalsQuery query,
            CancellationToken ct = default)
        {
            var today = DateTime.UtcNow.Date;
            var habitsTask = _habitRepository.GetActiveAsync(ct);
            var logsTask = _logRepository.GetRangeAsync(today.AddDays(-6), today, ct);
            await Task.WhenAll(habitsTask, logsTask);

            return _metricsService.Calculate(
                await habitsTask,
                await logsTask,
                today);
        }
    }
}
