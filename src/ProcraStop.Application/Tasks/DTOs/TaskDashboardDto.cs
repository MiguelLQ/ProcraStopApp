


using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.Application.Tasks.DTOs
{
    public sealed class TaskDashboardDto
    {
        public Guid Id { get; init; }

        public TaskItemStatus Status { get; init; }

        public bool IsArchived { get; init; }

        public DateTime? DueDate { get; init; }

        public int? EstimatedMinutes { get; init; }
    }
}
