

using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.DTOs;

namespace ProcraStop.Modules.Tasks.Application.Queries.GetTaskById
{
    public sealed class GetTaskByIdQueryHandler
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ICourseRepository _courseRepository;

        public GetTaskByIdQueryHandler(
            ITaskRepository taskRepository,
            ICourseRepository courseRepository)
        {
            _taskRepository = taskRepository;
            _courseRepository = courseRepository;
        }

        public async Task<TaskDetailDto?> HandleAsync(
            GetTaskByIdQuery query,
            CancellationToken ct = default)
        {
            var task = await _taskRepository.GetByIdAsync(query.TaskId, ct);

            if (task is null)
                return null;

            string? courseName = null;

            if (task.CourseId.HasValue)
            {
                var course = await _courseRepository.GetByIdAsync(task.CourseId.Value, ct);
                courseName = course?.Name;
            }

            return TaskDetailDto.FromDomain(task, courseName);
        }
    }
}
