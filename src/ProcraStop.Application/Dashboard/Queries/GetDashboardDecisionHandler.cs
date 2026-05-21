

using ProcraStop.Application.Dashboard.Interfaces;
using ProcraStop.Application.Dashboard.Models;
namespace ProcraStop.Application.Dashboard.Queries
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
