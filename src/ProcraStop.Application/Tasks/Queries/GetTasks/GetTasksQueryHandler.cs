
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.DTOs;

namespace ProcraStop.Application.Tasks.Queries.GetTasks
{
    public class GetTasksQueryHandler
    {
        private readonly ITaskReadService _readService;

        public GetTasksQueryHandler(ITaskReadService readService)
        {
            _readService = readService;
        }

        public async Task<IReadOnlyList<TaskListItemDto>> HandleAsync(
            GetTasksQuery query,
            CancellationToken cancellationToken = default)
        {
            return await _readService.GetListAsync(
                query.IncludeArchived,
                cancellationToken);
        }
    }
}
