
using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.DTOs;
using ProcraStop.Modules.Tasks.Domain.Entities;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.Queries.GetTodayRecommendations
{
    public class GetTodayRecommendationsHandler
    {
        private readonly ITaskRepository _repository;

        public GetTodayRecommendationsHandler(ITaskRepository repository)
        {
            _repository = repository;
        }
        public async Task<List<RecommendedTaskDto>> Handle(GetTodayRecommendationsQuery query)
        {
            var tasks = await _repository.GetAllAsync();

            var today = DateTime.Today;

            var result = tasks
                .Where(t => !t.IsCompleted && !t.IsArchived)
                .Select(t => new
                {
                    Task = t,
                    Score = CalculateScore(t, today)
                })
                .OrderByDescending(x => x.Score)
                .Take(5)
                .Select(x => new RecommendedTaskDto
                {
                    Id = x.Task.Id,
                    Title = x.Task.Title,
                    EstimatedMinutes = x.Task.EstimatedMinutes,
                    Effort = x.Task.GetEffort()
                })
                .ToList();

            return result;
        }

        private int CalculateScore(StudyTask task, DateTime today)
        {
            int score = 0;

            // Urgencia
            if (task.DueDate.HasValue)
            {
                var diff = (task.DueDate.Value.Date - today).Days;

                score += diff switch
                {
                    < 0 => 100,
                    0 => 80,
                    1 => 60,
                    _ => 20
                };
            }

            // Prioridad
            score += task.Priority switch
            {
                TaskPriority.High => 50,
                TaskPriority.Medium => 30,
                TaskPriority.Low => 10,
                _ => 0
            };

            // ESFUERZO (usa tu dominio)
            score += task.GetEffort() switch
            {
                TaskEffort.Quick => 30,
                TaskEffort.Medium => 20,
                TaskEffort.Heavy => 10,
                _ => 0
            };

            return score;
        }
    }
}
