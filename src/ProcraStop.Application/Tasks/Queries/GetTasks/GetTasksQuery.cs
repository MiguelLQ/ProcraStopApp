

namespace ProcraStop.Application.Tasks.Queries.GetTasks
{
    public sealed class GetTasksQuery
    {
        public bool IncludeArchived { get; init; }
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 50;
    }
}
