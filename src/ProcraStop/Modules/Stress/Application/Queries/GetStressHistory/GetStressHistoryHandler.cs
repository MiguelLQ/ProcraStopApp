using ProcraStop.Modules.Stress.Application.Abstractions;
using ProcraStop.Modules.Stress.Application.DTOs;

namespace ProcraStop.Modules.Stress.Application.Queries.GetStressHistory
{
    public class GetStressHistoryHandler
    {
        private readonly IStressRepository _repo;

        public GetStressHistoryHandler(IStressRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<StressDto>> Handle(GetStressHistoryQuery q)
        {
            var data = await _repo.GetRecentAsync(q.Days);

            return data.Select(x => new StressDto
            {
                Level = x.Level,
                Timestamp = x.Timestamp
            }).ToList();
        }
    }
}
