

using ProcraStop.Modules.Dashboard.Application.Models;
using ProcraStop.Modules.Dashboard.Application.Services;

namespace ProcraStop.Modules.Dashboard.Application.Queries
{

    public class GetDashboardDecisionHandler
    {
        private readonly IDashboardProjectionService _service;

        public GetDashboardDecisionHandler(IDashboardProjectionService service)
        {
            _service = service;
        }

        public Task<DashboardReadModel> Handle(GetDashboardDecisionQuery query)
        {
            return _service.BuildAsync(CancellationToken.None);
        }
    }
}
