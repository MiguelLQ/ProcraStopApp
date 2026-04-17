
namespace ProcraStop.Modules.Tasks.Application.Commands.ToggleSubtask
{
    public sealed class ToggleSubtaskCommand
    {
        public Guid TaskId { get; set; }
        public Guid SubtaskId { get; set; }
    }
}
