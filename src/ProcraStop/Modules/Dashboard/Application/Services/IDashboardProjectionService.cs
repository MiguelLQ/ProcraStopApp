
using ProcraStop.Modules.Dashboard.Application.Models;

namespace ProcraStop.Modules.Dashboard.Application.Services
{
    public interface IDashboardProjectionService
    {
        Task<DashboardReadModel> BuildAsync();

    }
}
