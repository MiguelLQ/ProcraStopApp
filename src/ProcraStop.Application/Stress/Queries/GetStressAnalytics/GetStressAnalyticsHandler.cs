

using ProcraStop.Application.Stress.Abstractions;
using ProcraStop.Application.Stress.DTOs;
using ProcraStop.Application.Stress.Services;

namespace ProcraStop.Application.Stress.Queries.GetStressAnalytics
{
    public class GetStressAnalyticsHandler
    {
        private readonly IStressRepository _repo;
        private readonly StressAnalyticsService _service;

        public GetStressAnalyticsHandler(
            IStressRepository repo,
            StressAnalyticsService service)
        {
            _repo = repo;
            _service = service;
        }

        public async Task<StressAnalyticsDto> Handle(GetStressAnalyticsQuery q)
        {
            var data = await _repo.GetRecentAsync(q.Days);

            if (!data.Any())
                return new StressAnalyticsDto();

            var ordered = data
                .OrderBy(x => x.Timestamp)
                .ToList();

            var levels = ordered.Select(x => x.Level).ToList();

            return new StressAnalyticsDto
            {
                Average = levels.Average(),
                Max = levels.Max(),
                Min = levels.Min(),
                RecoveryScore = 1 - levels.Average(),
                HighStressDays = ordered.Count(x => x.Level > 0.7),
                Trend = _service.CalculateTrend(levels),
                Points = ordered.Select(x => new StressPointDto
                {
                    Date = x.Timestamp,
                    Level = x.Level
                }).ToList()
            };
        }
    }
}
