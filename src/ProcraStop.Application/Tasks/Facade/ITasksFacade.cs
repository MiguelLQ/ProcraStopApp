
using ProcraStop.Application.Tasks.DTOs;

namespace ProcraStop.Application.Tasks.Facade
{
    public interface ITasksFacade
    {
        // metodo para obtener la lista de tareas, con opcion de incluir las archivadas
        Task<IReadOnlyList<TaskListItemDto>> GetTasksAsync(
            bool includeArchived = false,
            CancellationToken ct = default);
    }
}
