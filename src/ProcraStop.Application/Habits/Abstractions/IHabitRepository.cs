

using ProcraStop.Domain.Habits.Entities;

namespace ProcraStop.Application.Habits.Abstractions
{
    public interface IHabitRepository
    {
        Task<List<Habit>> GetAllAsync();
        Task AddAsync(Habit habit);
    }
}
