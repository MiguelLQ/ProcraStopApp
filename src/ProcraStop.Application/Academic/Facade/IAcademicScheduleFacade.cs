using ProcraStop.Application.Academic.DTOs;

namespace ProcraStop.Application.Academic.Facade;

public interface IAcademicScheduleFacade
{
    Task<IReadOnlyList<ClassScheduleDto>> GetScheduleAsync(DayOfWeek? dayOfWeek = null, CancellationToken ct = default);
    Task<Guid> CreateScheduleAsync(CreateClassScheduleRequest request, CancellationToken ct = default);
    Task UpdateScheduleAsync(UpdateClassScheduleRequest request, CancellationToken ct = default);
    Task DeleteScheduleAsync(Guid id, CancellationToken ct = default);
}
