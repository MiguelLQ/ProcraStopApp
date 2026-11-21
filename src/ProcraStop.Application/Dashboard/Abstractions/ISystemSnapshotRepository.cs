

using ProcraStop.Application.Dashboard.Models.Historial;

namespace ProcraStop.Application.Dashboard.Abstractions

{
    public interface ISystemSnapshotRepository
    {
        Task<List<SystemSnapshot>> GetLastDays(int days, CancellationToken ct = default);
        Task SaveAsync(SystemSnapshot snapshot, CancellationToken ct = default);
    }
}
