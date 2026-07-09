

using ProcraStop.Domain.Habits.Entities;

namespace ProcraStop.Application.Habits.Abstractions
{
    public interface IHabitRepository
    {
        Task<List<Habit>> GetAllAsync();
        Task AddAsync(Habit habit);
        Task<IReadOnlyList<Habit>> GetActiveAsync(CancellationToken ct);
        Task<Habit?> GetByIdAsync(Guid id, CancellationToken ct);
        Task UpdateAsync(Habit habit, CancellationToken ct);
    }
}
