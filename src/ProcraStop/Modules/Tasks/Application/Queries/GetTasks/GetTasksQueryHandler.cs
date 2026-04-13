using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.DTOs;
using ProcraStop.Modules.Tasks.Domain.Entities;
using ProcraStop.Modules.Tasks.Infraestructure.Repositories;


namespace ProcraStop.Modules.Tasks.Application.Queries.GetTasks
{
    public sealed class GetTasksQueryHandler
    {
        private readonly ITaskRepository _repository;
        private readonly ICourseRepository _courseRepository;

        public GetTasksQueryHandler(ITaskRepository repository, ICourseRepository courseRepository)
        {
            _repository = repository;
            _courseRepository = courseRepository;
        }

        public async Task<IReadOnlyList<TaskListItemDto>> HandleAsync(
    GetTasksQuery query,
    CancellationToken cancellationToken = default)
        {
            // 1. cargar datos
            var tasks = await _repository.GetAllAsync(cancellationToken);
            var courses = await _courseRepository.GetAllAsync(cancellationToken);
            var courseDict = courses.ToDictionary(c => c.Id);

            foreach (var t in tasks)
            {
                Console.WriteLine($"TASK: {t.Title} | COURSEID: {t.CourseId}");
            }

            var filtered = tasks
                .Where(x => query.IncludeArchived || !x.IsArchived)
                .Select(task =>
                {
                    Course? course = null;

                    if (task.CourseId.HasValue)
                    {
                        courseDict.TryGetValue(task.CourseId.Value, out course);
                    }

                    Console.WriteLine($"JOIN TASK={task.Title} COURSE={(course?.Name ?? "NULL")}");

                    return new TaskListItemDto
                    {
                        Id = task.Id,
                        Title = task.Title,
                        Description = task.Description,
                        DueDate = task.DueDate,
                        Priority = task.Priority,
                        Status = task.Status,
                        IsArchived = task.IsArchived,
                        EstimatedMinutes = task.EstimatedMinutes,
                        IsOverdue = task.IsOverdue,

                        CourseId = task.CourseId,
                        CourseName = course?.Name,
                        CourseColor = course?.Color
                    };
                })
                .ToList();

            return filtered;
        }
    }
}
