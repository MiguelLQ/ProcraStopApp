using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.DTOs;

namespace ProcraStop.Application.Tasks.Queries.GetTaskDaySummaries
{
    // editbycodex: handler CQRS dedicado a los indicadores diarios del calendario.
    public sealed class GetTaskDaySummariesQueryHandler
    {
        private readonly ITaskReadService _readService;

        public GetTaskDaySummariesQueryHandler(ITaskReadService readService)
        {
            _readService = readService;
        }

        public Task<IReadOnlyList<TaskDaySummaryDto>> HandleAsync(
            GetTaskDaySummariesQuery query,
            CancellationToken cancellationToken = default)
        {
            return _readService.GetDaySummariesAsync(
                query.StartDate.Date,
                query.EndDate.Date,
                cancellationToken);
        }
    }
}
