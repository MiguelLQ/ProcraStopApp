using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.DTOs;

namespace ProcraStop.Application.Tasks.Queries.SearchTasks
{
    public sealed class SearchTasksQueryHandler
    {
        private readonly ITaskReadService _readService;

        public SearchTasksQueryHandler(ITaskReadService readService)
        {
            _readService = readService;
        }

        public Task<IReadOnlyList<TaskListItemDto>> HandleAsync(
            SearchTasksQuery query,
            CancellationToken cancellationToken = default)
        {
            // editbycodex: query CQRS dedicada para mantener busqueda fuera de la UI.
            return _readService.SearchAsync(
                query.SearchText,
                query.IncludeArchived,
                query.DueDate,
                query.WithoutDueDate,
                query.MaxResults,
                cancellationToken);
        }
    }
}
