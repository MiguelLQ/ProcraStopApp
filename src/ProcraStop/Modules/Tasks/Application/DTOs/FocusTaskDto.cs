using ProcraStop.Modules.Tasks.Application.EngineTask.DTOs;

namespace ProcraStop.Modules.Tasks.Application.DTOs
{
    public class FocusTaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Score { get; set; }
        public TaskEffort Effort { get; set; }
        public int? EstimatedMinutes { get; set; }
    }
}
