using ProcraStop.Modules.Tasks.Domain.Entities;

namespace ProcraStop.Modules.Tasks.Application.Abstractions
{
    public interface ITaskRepository
    {
        // Define métodos para interactuar con la fuente de datos de tareas, como obtener todas las tareas,
        // obtener una tarea por su ID, agregar una nueva tarea y actualizar una tarea existente.
        Task<IReadOnlyList<StudyTask>> GetAllAsync(CancellationToken cancellationToken = default);
        // El método GetByIdAsync devuelve una tarea específica basada en su ID. Si no se encuentra la tarea, devuelve null.
        Task<StudyTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        // El método AddAsync se utiliza para agregar una nueva tarea a la fuente de datos.
        // Recibe un objeto StudyTask que representa la tarea a agregar.
        Task AddAsync(StudyTask task, CancellationToken cancellationToken = default);
        // El método UpdateAsync se utiliza para actualizar una tarea existente en la fuente de datos.
        Task UpdateAsync(StudyTask task, CancellationToken cancellationToken = default);
    }
}
