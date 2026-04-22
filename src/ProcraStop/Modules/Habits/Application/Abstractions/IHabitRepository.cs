

using ProcraStop.Modules.Habits.Domain.Entities;

namespace ProcraStop.Modules.Habits.Application.Abstractions
{
    public interface IHabitRepository
    {
        Task<List<Habit>> GetAllAsync();
        Task AddAsync(Habit habit);
    }
}
