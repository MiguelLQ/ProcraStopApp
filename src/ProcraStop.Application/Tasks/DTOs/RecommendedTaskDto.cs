
using ProcraStop.Application.Tasks.EngineTask.DTOs;

namespace ProcraStop.Application.Tasks.DTOs
{
    public class RecommendedTaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? EstimatedMinutes { get; set; }
        public TaskEffort Effort { get; set; }
    }
}
