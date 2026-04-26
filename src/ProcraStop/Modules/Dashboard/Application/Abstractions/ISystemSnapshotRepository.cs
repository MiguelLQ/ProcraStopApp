
using ProcraStop.Modules.Dashboard.Application.Models.Historial;

namespace ProcraStop.Modules.Dashboard.Application.Abstractions
{
    public interface ISystemSnapshotRepository
    {
        Task<List<SystemSnapshot>> GetLastDays(int days);
        Task SaveAsync(SystemSnapshot snapshot);
    }
}
