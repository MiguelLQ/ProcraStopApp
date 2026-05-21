

using ProcraStop.Application.Dashboard.Models.Historial;

namespace ProcraStop.Application.Dashboard.Abstractions

{
    public interface ISystemSnapshotRepository
    {
        Task<List<SystemSnapshot>> GetLastDays(int days);
        Task SaveAsync(SystemSnapshot snapshot);
    }
}
