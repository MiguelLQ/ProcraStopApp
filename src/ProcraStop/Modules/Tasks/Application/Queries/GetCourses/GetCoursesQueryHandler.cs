
using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.DTOs;

namespace ProcraStop.Modules.Tasks.Application.Queries.GetCourses
{
    public sealed class GetCoursesQueryHandler
    {
        private readonly ICourseRepository _courseRepository;

        public GetCoursesQueryHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<IReadOnlyList<CourseDto>> HandleAsync(
            GetCoursesQuery query,
            CancellationToken ct = default)
        {
            var courses = await _courseRepository.GetAllAsync(ct);

            return courses
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Color = c.Color
                })
                .ToList();
        }
    }
}
