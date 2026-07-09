
namespace ProcraStop.Modules.Tasks.Presentation.Models
{
    public enum TaskFilterOption
    {
        All = 1,
        Pending = 2,
        Completed = 3,
        Overdue = 4,
        WithoutDate = 5 // editbycodex: bandeja explicita para tareas aun no programadas.
    }
}
