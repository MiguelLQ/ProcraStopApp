

using ProcraStop.Application.Academic.DTOs;

namespace ProcraStop.Application.Academic.Facade
{
    public interface ICoursesFacade
    {
        Task<IReadOnlyList<CourseDto>> GetCoursesAsync(CancellationToken ct = default);

        Task<Guid> CreateCourseAsync(CreateCourseRequest request, CancellationToken ct = default);

        Task UpdateCourseAsync(UpdateCourseRequest request, CancellationToken ct = default);

        Task DeleteCourseAsync(Guid id, CancellationToken ct = default);
    }
}
