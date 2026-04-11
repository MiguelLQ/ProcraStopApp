using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.DTOs;


namespace ProcraStop.Modules.Tasks.Application.Queries.GetTasks
{
    public sealed class GetTasksQueryHandler
    {
        private readonly ITaskRepository _repository;

        public GetTasksQueryHandler(ITaskRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<TaskListItemDto>> HandleAsync(
            GetTasksQuery query,
            CancellationToken cancellationToken = default)
        {
            var tasks = await _repository.GetAllAsync(cancellationToken);

            var now = DateTime.Now;

            var filtered = tasks
                .Where(x => query.IncludeArchived || !x.IsArchived)
                .OrderBy(x => x.Status == Domain.Enums.TaskItemStatus.Completed)
                .ThenBy(x => x.DueDate.HasValue ? 0 : 1)
                .ThenBy(x =>
                    x.Status == Domain.Enums.TaskItemStatus.Completed
                        ? DateTime.MaxValue
                        : (x.DueDate ?? DateTime.MaxValue))
                .ThenByDescending(x => x.Priority)
                .Select(x => new TaskListItemDto
    {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    DueDate = x.DueDate,
                    Priority = x.Priority,
                    Status = x.Status,
                    IsArchived = x.IsArchived,
                    EstimatedMinutes = x.EstimatedMinutes,
                    IsOverdue = x.IsOverdue
                })
                .ToList();

            return filtered;
        }
    }
}
