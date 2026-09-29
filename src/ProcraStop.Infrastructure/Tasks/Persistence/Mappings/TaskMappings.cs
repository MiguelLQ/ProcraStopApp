using ProcraStop.Domain.Tasks.Entities;
using ProcraStop.Domain.Tasks.Enums;
using ProcraStop.Infrastructure.Tasks.Persistence.Entities;


namespace ProcraStop.Infrastructure.Tasks.Persistence.Mappings
{
    public static class TaskMappings
    {
        // Método para convertir de dominio a entidad, incluyendo subtareas
        public static StudyTaskEntity ToEntity(this StudyTask task)
        {
            return new StudyTaskEntity
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                ReminderDate = task.ReminderDate,
                Priority = (int)task.Priority,
                Status = (int)task.Status,
                IsArchived = task.IsArchived,
                CourseId = task.CourseId,
                EstimatedMinutes = task.EstimatedMinutes,
                CreatedAtUtc = task.CreatedAtUtc,
                CompletedAtUtc = task.CompletedAtUtc,
                ActivityType = (int)task.ActivityType,
                PlannedStartDate = task.PlannedStartDate,
                StartedAtUtc = task.StartedAtUtc,
                FirstStep = task.FirstStep,
                PlannedReviewDate = task.PlannedReviewDate,
                ReviewedAtUtc = task.ReviewedAtUtc,
                // SUBTASKS
                Subtasks = (task.Subtasks ?? [])
                .Select(s => new SubtaskEntity
                {
                    Id = s.Id,
                    Title = s.Title,
                    IsCompleted = s.IsCompleted,
                    PlannedDate = s.PlannedDate,
                    TaskId = task.Id
                })
                .ToList()
            };
        }
        // Método para convertir de entidad a dominio, incluyendo subtareas
        public static StudyTask ToDomain(this StudyTaskEntity entity)
        {
            var subtasks = (entity.Subtasks ?? new List<SubtaskEntity>())
                .Select(s => new Subtask(
                    s.Id,
                    s.Title,
                    s.IsCompleted,
                    s.PlannedDate
                ))
                .ToList();

            return StudyTask.FromPersistence(
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
                subtasks,
                (AcademicActivityType)entity.ActivityType,
                entity.PlannedStartDate,
                entity.StartedAtUtc,
                entity.FirstStep,
                entity.PlannedReviewDate,
                entity.ReviewedAtUtc
            );
        }
    }
}
