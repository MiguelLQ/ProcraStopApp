using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.DTOs;

namespace ProcraStop.Modules.Tasks.Application.Queries.GetTasks
{

    public sealed class GetTasksQueryHandler
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
