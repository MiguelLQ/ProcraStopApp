
using ProcraStop.Modules.Stress.Domain.Entities;

namespace ProcraStop.Modules.Stress.Application.Abstractions
{
    public interface IStressRepository
    {
        Task AddAsync(StressRecord record);
        Task<List<StressRecord>> GetRecentAsync(int days);
        Task<StressRecord?> GetLatestAsync();
    }
}
