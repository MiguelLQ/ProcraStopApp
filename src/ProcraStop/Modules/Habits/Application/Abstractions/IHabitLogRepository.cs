
using ProcraStop.Modules.Habits.Domain.Entities;

namespace ProcraStop.Modules.Habits.Application.Abstractions
{
    public interface IHabitLogRepository
    {
        Task<List<HabitLog>> GetAllAsync();
        Task AddAsync(HabitLog log);
        // 
        Task<HabitLog?> GetByHabitAndDateAsync(Guid habitId, DateTime date);
        //
        Task UpsertAsync(HabitLog log);

    }
}
