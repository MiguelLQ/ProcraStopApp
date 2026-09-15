
using Microsoft.EntityFrameworkCore;
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.Analytics;
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Domain.Tasks.Enums;
using ProcraStop.Infrastructure.DBContext;

namespace ProcraStop.Infraestructure.Tasks.Services
{
    public sealed class TaskReadService : ITaskReadService
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public TaskReadService(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
        public async Task<TaskDetailDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
            return await db.Tasks
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
            await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
            var query = db.Tasks
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
                    Description = t.Description, // editbycodex: mantiene completo el contrato de busqueda.

                    DueDate = t.DueDate,

                    Priority = (TaskPriority)t.Priority,

                    Status = (TaskItemStatus)t.Status,

                    IsArchived = t.IsArchived,

                    EstimatedMinutes = t.EstimatedMinutes,
                    HasSubtasks = t.Subtasks.Any(), // editbycodex: no materializa subtareas.

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

        public async Task<IReadOnlyList<TaskListItemDto>> SearchAsync(
            string searchText,
            bool includeArchived,
            DateTime? dueDate,
            bool withoutDueDate,
            int maxResults,
            CancellationToken ct)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
            var normalized = searchText.Trim();

            var query = db.Tasks
                .AsNoTracking();

            if (!includeArchived)
                query = query.Where(x => !x.IsArchived);

            if (withoutDueDate)
            {
                query = query.Where(x => !x.DueDate.HasValue); // editbycodex: busqueda dentro de tareas sin programar.
            }
            else if (dueDate.HasValue)
            {
                var dayStart = dueDate.Value.Date;
                var dayEnd = dayStart.AddDays(1);
                query = query.Where(x =>
                    x.DueDate.HasValue &&
                    x.DueDate.Value >= dayStart &&
                    x.DueDate.Value < dayEnd); // editbycodex: filtro de fecha ejecutado en base de datos.
            }

            if (!string.IsNullOrWhiteSpace(normalized))
            {
                // editbycodex: busqueda en capa de lectura; cubre titulo, descripcion y curso sin cargar todo a UI.
                query = query.Where(t =>
                    EF.Functions.Like(t.Title, $"%{normalized}%") ||
                    (t.Description != null && EF.Functions.Like(t.Description, $"%{normalized}%")) ||
                    (t.Course != null && EF.Functions.Like(t.Course.Name, $"%{normalized}%")));
            }

            return await query
                .OrderBy(x => x.Status == (int)TaskItemStatus.Completed)
                .ThenBy(x => x.DueDate ?? DateTime.MaxValue)
                .ThenByDescending(x => x.CreatedAtUtc)
                .Take(maxResults)
                .Select(t => new TaskListItemDto
                {
                    Id = t.Id,

                    Title = t.Title,
                    Description = t.Description, // editbycodex: habilita busqueda local sin otra lectura.

                    DueDate = t.DueDate,

                    Priority = (TaskPriority)t.Priority,

                    Status = (TaskItemStatus)t.Status,

                    IsArchived = t.IsArchived,

                    EstimatedMinutes = t.EstimatedMinutes,
                    HasSubtasks = t.Subtasks.Any(), // editbycodex: EXISTS en SQL, sin cargar la coleccion.

                    IsOverdue =
                        t.DueDate.HasValue &&
                        t.DueDate.Value < DateTime.Now &&
                        t.Status != (int)TaskItemStatus.Completed &&
                        !t.IsArchived,

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

        public async Task<IReadOnlyList<TaskDaySummaryDto>> GetDaySummariesAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
            var rangeStart = startDate.Date;
            var rangeEndExclusive = endDate.Date.AddDays(1);
            var now = DateTime.Now;

            // editbycodex: una consulta minima por rango; el agrupado local evita incompatibilidades Date de SQLite.
            var rows = await db.Tasks
                .AsNoTracking()
                .Where(x =>
                    !x.IsArchived &&
                    x.DueDate.HasValue &&
                    x.DueDate.Value >= rangeStart &&
                    x.DueDate.Value < rangeEndExclusive)
                .Select(x => new
                {
                    DueDate = x.DueDate!.Value,
                    x.Status
                })
                .ToListAsync(ct);

            return rows
                .GroupBy(x => x.DueDate.Date)
                .Select(group => new TaskDaySummaryDto
                {
                    Date = group.Key,
                    TaskCount = group.Count(),
                    OverdueCount = group.Count(x =>
                        x.DueDate < now &&
                        x.Status != (int)TaskItemStatus.Completed)
                })
                .OrderBy(x => x.Date)
                .ToList();
        }

        public async Task<IReadOnlyList<TaskDashboardDto>> GetDashboardDataAsync(
    CancellationToken ct)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
            return await db.Tasks
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

        public async Task<IReadOnlyList<TaskAnalyticsSourceItemDto>> GetAnalyticsDataAsync(
            CancellationToken ct)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
            // editbycodex: una sola consulta sin tracking, textos grandes ni navegaciones completas.
            return await db.Tasks
                .AsNoTracking()
                .Where(x => !x.IsArchived)
                .Select(x => new TaskAnalyticsSourceItemDto
                {
                    DueDate = x.DueDate,
                    CompletedAtUtc = x.CompletedAtUtc,
                    Priority = (TaskPriority)x.Priority,
                    Status = (TaskItemStatus)x.Status,
                    EstimatedMinutes = x.EstimatedMinutes,
                    CourseName = x.Course != null ? x.Course.Name : null
                })
                .ToListAsync(ct);
        }
    }
}
