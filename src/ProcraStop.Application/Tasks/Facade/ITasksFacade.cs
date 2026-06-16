
using ProcraStop.Application.Tasks.Commands.UpdateTask;
using ProcraStop.Application.Tasks.DTOs;

namespace ProcraStop.Application.Tasks.Facade
{
    public interface ITasksFacade
    {
        // metodo para obtener la lista de tareas, con opcion de incluir las archivadas
        Task<IReadOnlyList<TaskListItemDto>> GetTasksAsync(
            bool includeArchived = false,
            CancellationToken ct = default);
        Task<Guid> CreateTaskAsync(
            CreateCourseRequest request,
            CancellationToken ct = default);
        Task CompleteTaskAsync(
            Guid taskId,
            CancellationToken ct = default);
        Task ArchiveTaskAsync(
           Guid taskId,
           CancellationToken ct = default);
        Task<IReadOnlyList<CourseDto>> GetCoursesAsync(
            CancellationToken ct = default);
        Task<TaskDetailDto?> GetTaskByIdAsync(
            Guid taskId,
            CancellationToken ct = default);
        Task UpdateTaskAsync(
            Guid taskId,
            UpdateTaskCommand request,
            CancellationToken ct = default);
    }
}
