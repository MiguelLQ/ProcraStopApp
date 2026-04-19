
using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.DTOs;
using ProcraStop.Modules.Tasks.Domain.Entities;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.Queries.GetFocusTasks
{
    public class GetFocusTasksHandler
    {
        private readonly ITaskRepository _repository;

        public GetFocusTasksHandler(ITaskRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<FocusTaskDto>> Handle(GetFocusTasksQuery query)
        {
            var tasks = await _repository.GetAllAsync();

            var pending = tasks
                .Where(t => !t.IsCompleted && !t.IsArchived)
                .ToList();

            var result = pending
                .Select(t => new
                {
                    Task = t,
                    Score = CalculateFocusScore(t)
                })
                .OrderByDescending(x => x.Score)
                .Take(3) // 👈 CLAVE: foco real
                .Select(x => new FocusTaskDto
                {
                    Id = x.Task.Id,
                    Title = x.Task.Title,
                    EstimatedMinutes = x.Task.EstimatedMinutes,
                    Effort = x.Task.GetEffort(),
                    Score = x.Score
                })
                .ToList();

            return result;
        }

        private int CalculateFocusScore(StudyTask task)
        {
            int score = 0;

            var today = DateTime.Today;

            //  urgencia
            if (task.DueDate.HasValue)
            {
                var diff = (task.DueDate.Value.Date - today).Days;

                score += diff switch
                {
                    < 0 => 120,
                    0 => 100,
                    1 => 70,
                    _ => 20
                };
            }

            // prioridad
            score += task.Priority switch
            {
                TaskPriority.High => 50,
                TaskPriority.Medium => 30,
                TaskPriority.Low => 10,
                _ => 0
            };

            // esfuerzo (dominio puro)
            score += task.GetEffort() switch
            {
                TaskEffort.Quick => 40,   // 👈 clave para productividad rápida
                TaskEffort.Medium => 25,
                TaskEffort.Heavy => 10,
                _ => 0
            };

            return score;
        }
    }
}
