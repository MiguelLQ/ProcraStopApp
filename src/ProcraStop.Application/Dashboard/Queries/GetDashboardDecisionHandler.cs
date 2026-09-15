

using ProcraStop.Application.Dashboard.Interfaces;
using ProcraStop.Application.Dashboard.Models;
namespace ProcraStop.Application.Dashboard.Queries
{

    public class GetDashboardDecisionHandler
    {
        private readonly IDashboardProjectionService _service;
        private readonly IDashboardProjectionCache _cache;

        public GetDashboardDecisionHandler(
            IDashboardProjectionService service,
            IDashboardProjectionCache cache)
        {
            _service = service;
            _cache = cache;
        }

        public Task<DashboardReadModel> Handle(
            GetDashboardDecisionQuery query,
            CancellationToken cancellationToken = default)
        {
            return _cache.GetOrCreateAsync(_service.BuildAsync, cancellationToken);
        }
    }
}
