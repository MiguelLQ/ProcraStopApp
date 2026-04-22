
using Microsoft.EntityFrameworkCore;
using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.DTOs;
using ProcraStop.Modules.Tasks.Infraestructure.Persistence;

namespace ProcraStop.Modules.Tasks.Infraestructure.Services
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

                    Priority = (int)t.Priority,
                    Status = (int)t.Status,

                    EstimatedMinutes = t.EstimatedMinutes,

                    CourseId = t.CourseId,
                    CourseName = t.Course != null ? t.Course.Name : null,
                    CourseColor = t.Course != null ? t.Course.Color : null,
                    Subtasks = t.Subtasks.Select(s => new SubtaskDto
                    {
                        Id = s.Id,
                        Title = s.Title,
                        IsCompleted = s.IsCompleted
                    }).ToList()
                })
                .FirstOrDefaultAsync(ct);
        }
        public async Task<IReadOnlyList<TaskDetailDto>> GetAllAsync(CancellationToken ct)
        {
            return await _db.Tasks
                .AsNoTracking()
                .Select(t => new TaskDetailDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    ReminderDate = t.ReminderDate,
                    Priority = (int)t.Priority,
                    Status = (int)t.Status,
                    EstimatedMinutes = t.EstimatedMinutes,
                    CourseId = t.CourseId,
                    CourseName = t.Course != null ? t.Course.Name : null,
                    CourseColor = t.Course != null ? t.Course.Color : null,
                    Subtasks = t.Subtasks.Select(s => new SubtaskDto
                    {
                        Id = s.Id,
                        Title = s.Title,
                        IsCompleted = s.IsCompleted
                    }).ToList()
                })
                .ToListAsync(ct);
        }

    }
}
