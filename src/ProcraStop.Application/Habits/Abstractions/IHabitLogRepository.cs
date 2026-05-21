

using ProcraStop.Domain.Habits.Entities;

namespace ProcraStop.Application.Habits.Abstractions
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
