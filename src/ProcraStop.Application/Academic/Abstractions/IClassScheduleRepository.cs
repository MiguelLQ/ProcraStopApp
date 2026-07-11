using ProcraStop.Domain.Academic.Entities;

namespace ProcraStop.Application.Academic.Abstractions;

public interface IClassScheduleRepository
{
    Task<IReadOnlyList<ClassSchedule>> GetAllActiveAsync(CancellationToken ct);
    Task<IReadOnlyList<ClassSchedule>> GetByDayAsync(DayOfWeek dayOfWeek, CancellationToken ct);
    Task<ClassSchedule?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(ClassSchedule schedule, CancellationToken ct);
    Task UpdateAsync(ClassSchedule schedule, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
