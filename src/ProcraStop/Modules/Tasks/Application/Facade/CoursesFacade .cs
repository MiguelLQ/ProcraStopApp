

using ProcraStop.Modules.Tasks.Application.Commands.CreateCourse;
using ProcraStop.Modules.Tasks.Application.Commands.DeleteCourse;
using ProcraStop.Modules.Tasks.Application.Commands.UpdateCourse;
using ProcraStop.Modules.Tasks.Application.DTOs;
using ProcraStop.Modules.Tasks.Application.Queries.GetCourses;

namespace ProcraStop.Modules.Tasks.Application.Facade
{
    public sealed class CoursesFacade : ICoursesFacade
    {
        private readonly GetCoursesQueryHandler _getCoursesHandler;
        private readonly CreateCourseCommandHandler _createHandler;
        private readonly UpdateCourseCommandHandler _updateHandler;
        private readonly DeleteCourseCommandHandler _deleteHandler;

        public CoursesFacade(
            GetCoursesQueryHandler getCoursesHandler,
            CreateCourseCommandHandler createHandler,
            UpdateCourseCommandHandler updateHandler,
            DeleteCourseCommandHandler deleteHandler)
        {
            _getCoursesHandler = getCoursesHandler;
            _createHandler = createHandler;
            _updateHandler = updateHandler;
            _deleteHandler = deleteHandler;
        }

        public Task<IReadOnlyList<CourseDto>> GetCoursesAsync(CancellationToken ct = default)
            => _getCoursesHandler.HandleAsync(new GetCoursesQuery(), ct);

        public Task<Guid> CreateCourseAsync(CreateCourseRequest request, CancellationToken ct = default)
        {
            var command = new CreateCourseCommand
            {
                Name = request.Name,
                Description = request.Description,
                Color = request.Color
            };

            return _createHandler.HandleAsync(command, ct);
        }

        public Task UpdateCourseAsync(UpdateCourseRequest request, CancellationToken ct = default)
        {
            var command = new UpdateCourseCommand
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description,
                Color = request.Color
            };

            return _updateHandler.HandleAsync(command, ct);
        }

        public Task DeleteCourseAsync(Guid id, CancellationToken ct = default)
    => _deleteHandler.HandleAsync(new DeleteCourseCommand { Id = id }, ct); 
    }
}
