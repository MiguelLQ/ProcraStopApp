
using Microsoft.EntityFrameworkCore;
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Domain.Tasks.Enums;
using ProcraStop.Infrastructure.DBContext;

namespace ProcraStop.Infraestructure.Tasks.Services
{
    public sealed class TaskReadService : ITaskReadService
    {
        private readonly AppDbContext _db;

        public TaskReadService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<TaskDetailDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await _db.Tasks
                .AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new TaskDetailDto
                {
                    Id = t.Id,

                    Title = t.Title,

                    Description = t.Description,

                    DueDate = t.DueDate,

                    ReminderDate = t.ReminderDate,

                    Priority = t.Priority,

                    Status = t.Status,

                    EstimatedMinutes = t.EstimatedMinutes,

                    CourseId = t.CourseId,

                    CourseName = t.Course != null
                        ? t.Course.Name
                        : null,

                    CourseColor = t.Course != null
                        ? t.Course.Color
                        : null,

                    Subtasks = t.Subtasks
                        .Select(s => new SubtaskDto
                        {
                            Id = s.Id,
                            Title = s.Title,
                            IsCompleted = s.IsCompleted
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<TaskListItemDto>> GetListAsync(bool includeArchived, CancellationToken ct)
        {
            var query = _db.Tasks
        .AsNoTracking();

            if (!includeArchived)
            {
                query = query.Where(x => !x.IsArchived);
            }

            return await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(t => new TaskListItemDto
                {
                    Id = t.Id,

                    Title = t.Title,

                    DueDate = t.DueDate,

                    Priority = (TaskPriority)t.Priority,

                    Status = (TaskItemStatus)t.Status,

                    IsArchived = t.IsArchived,

                    EstimatedMinutes = t.EstimatedMinutes,

                    IsOverdue =
                        t.DueDate.HasValue &&
                        t.DueDate.Value < DateTime.Now &&
                        t.Status != (int)TaskItemStatus.Completed &&
                        !t.IsArchived,

                    // COURSE

                    CourseId = t.CourseId,

                    CourseName = t.Course != null
                        ? t.Course.Name
                        : null,

                    CourseColor = t.Course != null
                        ? t.Course.Color
                        : null
                })
                .ToListAsync(ct);
        }
        public async Task<IReadOnlyList<TaskDashboardDto>> GetDashboardDataAsync(
    CancellationToken ct)
        {
            return await _db.Tasks
                .AsNoTracking()
                .Select(t => new TaskDashboardDto
                {
                    Id = t.Id,

                    Status = (TaskItemStatus)t.Status,

                    IsArchived = t.IsArchived,

                    DueDate = t.DueDate,

                    EstimatedMinutes = t.EstimatedMinutes
                })
                .ToListAsync(ct);
        }
    }
}
