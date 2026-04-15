

using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.DTOs;

namespace ProcraStop.Modules.Tasks.Application.Queries.GetTaskById
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