using ProcraStop.Modules.Tasks.Application.DTOs;

namespace ProcraStop.Modules.Tasks.Application.Facade
{
    public interface ITasksFacade
    {
        // metodo para obtener la lista de tareas, con opcion de incluir las archivadas
        Task<IReadOnlyList<TaskListItemDto>> GetTasksAsync(
            bool includeArchived = false,
            CancellationToken ct = default);

        // metodo para crear una nueva tarea
        Task<Guid> CreateTaskAsync(
            CreateTaskRequest request,
            CancellationToken ct = default);

        // metodo para completar una tarea
        Task CompleteTaskAsync(
            Guid taskId,
            CancellationToken ct = default);

        // metodo para archivar una tarea
        Task ArchiveTaskAsync(
            Guid taskId,
            CancellationToken ct = default);

        // metodo para obtener los cursos disponibles para asignar a una tarea
        Task<IReadOnlyList<CourseDto>> GetCoursesAsync(
        CancellationToken ct = default);
        // metodo para obtener los detalles de una tarea por su id
        Task<TaskDetailDto?> GetTaskByIdAsync(
            Guid taskId,
            CancellationToken ct = default);
        Task ToggleSubtaskAsync(Guid taskId,
            Guid subtaskId,
            CancellationToken ct = default);
    }
}
