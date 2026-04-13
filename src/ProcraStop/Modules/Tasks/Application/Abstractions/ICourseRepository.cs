

using ProcraStop.Modules.Tasks.Domain.Entities;

namespace ProcraStop.Modules.Tasks.Application.Abstractions
{
    public interface  ICourseRepository
    {
        Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken ct);
        Task<Course?> GetByIdAsync(Guid id, CancellationToken ct);
        Task AddAsync(Course course, CancellationToken ct);
        Task UpdateAsync(Course course, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);

    }
}
