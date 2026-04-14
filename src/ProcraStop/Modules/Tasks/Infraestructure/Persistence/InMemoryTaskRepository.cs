using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Domain.Entities;
using ProcraStop.Modules.Tasks.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
// este archivo es de prueba para simular un repositorio en memoria, no es parte de la solución final
namespace ProcraStop.Modules.Tasks.Infraestructure.Persistence
{
    public sealed class InMemoryTaskRepository : ITaskRepository
    {
        private readonly List<StudyTask> _tasks = new()
    {
        //new StudyTask
        //{
        //    Id = Guid.NewGuid(),
        //    Title = "Resolver ejercicios de cálculo",
        //    Description = "Capítulo 3 - derivadas",
        //    DueDate = DateTime.Now.AddDays(1),
        //    Priority = TaskPriority.High,
        //    Status = TaskItemStatus.Pending,
        //    IsArchived = false,
        //    EstimatedMinutes = 90,
        //    CreatedAtUtc = DateTime.UtcNow
        //},
        //new StudyTask
        //{
        //    Id = Guid.NewGuid(),
        //    Title = "Leer resumen de arquitectura",
        //    Description = "Repasar Clean + CQRS",
        //    DueDate = DateTime.Now.AddHours(-3),
        //    Priority = TaskPriority.Medium,
        //    Status = TaskItemStatus.Pending,
        //    IsArchived = false,
        //    EstimatedMinutes = 45,
        //    CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
        //}
    };

        public Task<IReadOnlyList<StudyTask>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult((IReadOnlyList<StudyTask>)_tasks.ToList());

        public Task<StudyTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_tasks.FirstOrDefault(x => x.Id == id));

        public Task AddAsync(StudyTask task, CancellationToken cancellationToken = default)
        {
            _tasks.Add(task);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(StudyTask task, CancellationToken cancellationToken = default)
        {
            var index = _tasks.FindIndex(x => x.Id == task.Id);

            if (index >= 0)
                _tasks[index] = task;

            return Task.CompletedTask;
        }
    }
}
