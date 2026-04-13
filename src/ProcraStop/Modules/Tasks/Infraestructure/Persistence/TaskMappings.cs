using ProcraStop.Modules.Tasks.Domain.Entities;
using ProcraStop.Modules.Tasks.Domain.Enums;


namespace ProcraStop.Modules.Tasks.Infraestructure.Persistence
{
    public static class TaskMappings
    {
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
                CompletedAtUtc = task.CompletedAtUtc
            };
        }

        public static StudyTask ToDomain(this StudyTaskEntity entity)
        {
            return new StudyTask
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                DueDate = entity.DueDate,
                ReminderDate = entity.ReminderDate,
                Priority = (TaskPriority)entity.Priority,
                Status = (TaskItemStatus)entity.Status,
                IsArchived = entity.IsArchived,
                CourseId = entity.CourseId,
                EstimatedMinutes = entity.EstimatedMinutes,
                CreatedAtUtc = entity.CreatedAtUtc,
                CompletedAtUtc = entity.CompletedAtUtc
            };
        }
    }
}
