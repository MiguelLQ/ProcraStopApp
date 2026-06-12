
namespace ProcraStop.Application.Tasks.Commands.ToggleSubtask
{
    public sealed class ToggleSubtaskCommand
    {
        public Guid TaskId { get; set; }
        public Guid SubtaskId { get; set; }
    }
}
