

using ProcraStop.Modules.Stress.Application.Abstractions;
using ProcraStop.Modules.Stress.Application.DTOs;
using ProcraStop.Modules.Stress.Application.Services;

namespace ProcraStop.Modules.Stress.Application.Queries.GetStressAnalytics
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

            return _service.Calculate(data);
        }
    }
}
