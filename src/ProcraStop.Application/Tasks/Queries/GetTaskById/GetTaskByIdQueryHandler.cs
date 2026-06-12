

using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.DTOs;

namespace ProcraStop.Application.Tasks.Queries.GetTaskById
{
    public sealed class GetTaskByIdQueryHandler
    {
        private readonly ITaskReadService _readService;

        public GetTaskByIdQueryHandler(ITaskReadService readService)
        {
            _readService = readService;
        }

        public Task<TaskDetailDto?> HandleAsync(
            GetTaskByIdQuery query,
            CancellationToken ct = default)
        {
            return _readService.GetByIdAsync(query.TaskId, ct);
        }

    }
}