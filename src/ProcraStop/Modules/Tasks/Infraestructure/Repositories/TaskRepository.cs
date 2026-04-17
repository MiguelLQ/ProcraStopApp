using Microsoft.EntityFrameworkCore;
using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Domain.Entities;
using ProcraStop.Modules.Tasks.Domain.Enums;
using ProcraStop.Modules.Tasks.Infraestructure.Persistence;

namespace ProcraStop.Modules.Tasks.Infraestructure.Repositories
{
    internal class TaskRepository : ITaskRepository
    {
        private readonly TasksDbContext _dbContext;

        public TaskRepository(TasksDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        // metodo para obtener todas las tareas, ordenadas por fecha de creación descendente
        public async Task<IReadOnlyList<StudyTask>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var items = await _dbContext.Tasks
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);

            return items.Select(x => x.ToDomain()).ToList();
        }
        // metodo para obtener una tarea por su id, incluyendo sus subtareas
        public async Task<StudyTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Tasks
                .Include(x => x.Subtasks) // OBLIGATORIO
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity is null)
                return null;

            //  MAPEO COMPLETO A DOMINIO
            var task = StudyTask.FromPersistence(
                entity.Id,
                entity.Title,
                entity.Description,
                entity.DueDate,
                entity.ReminderDate,
                (TaskPriority)entity.Priority,
                (TaskItemStatus)entity.Status,
                entity.IsArchived,
                entity.CourseId,
                entity.EstimatedMinutes,
                entity.CreatedAtUtc,
                entity.CompletedAtUtc,
                entity.Subtasks
                    .Select(s => new Subtask(s.Id, s.Title, s.IsCompleted))
                    .ToList()
            );

            return task;
        }

        public async Task AddAsync(StudyTask task, CancellationToken cancellationToken = default)
        {
            var entity = task.ToEntity();

            await _dbContext.Tasks.AddAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        // metodo para actualizar una tarea existente en la base de datos
        public async Task UpdateAsync(StudyTask task, CancellationToken cancellationToken = default)
        {
            var existing = await _dbContext.Tasks
                .Include(x => x.Subtasks) //IMPORTANTE
                .FirstOrDefaultAsync(x => x.Id == task.Id, cancellationToken);

            if (existing is null)
                return;

            // Update propiedades simples
            existing.Title = task.Title;
            existing.Description = task.Description;
            existing.DueDate = task.DueDate;
            existing.ReminderDate = task.ReminderDate;
            existing.Priority = (int)task.Priority;
            existing.Status = (int)task.Status;
            existing.IsArchived = task.IsArchived;
            existing.CourseId = task.CourseId;
            existing.EstimatedMinutes = task.EstimatedMinutes;
            //existing.CreatedAtUtc = task.CreatedAtUtc;
            existing.CompletedAtUtc = task.CompletedAtUtc;

            //SUBTASKS (LO IMPORTANTE)

            // 1. Eliminar las que ya no existen
            var incomingIds = task.Subtasks.Select(s => s.Id).ToHashSet();

            var toRemove = existing.Subtasks
                .Where(s => !incomingIds.Contains(s.Id))
                .ToList();

            _dbContext.Subtasks.RemoveRange(toRemove);

            // 2. Actualizar o agregar
            foreach (var subtask in task.Subtasks)
            {
                var existingSubtask = existing.Subtasks
                    .FirstOrDefault(s => s.Id == subtask.Id);

                if (existingSubtask is null)
                {
                    // nueva
                    existing.Subtasks.Add(new SubtaskEntity
                    {
                        Id = subtask.Id,
                        Title = subtask.Title,
                        IsCompleted = subtask.IsCompleted,
                        TaskId = task.Id
                    });
                }
                else
                {
                    // update
                    existingSubtask.Title = subtask.Title;
                    existingSubtask.IsCompleted = subtask.IsCompleted;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

    }
}
