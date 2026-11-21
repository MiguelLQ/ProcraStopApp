using Microsoft.EntityFrameworkCore;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Infrastructure.Tasks.Persistence.Entities;
using ProcraStop.Infrastructure.Tasks.Persistence.Mappings;
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Domain.Tasks.Entities;
using ProcraStop.Application.Dashboard.Interfaces;
using ProcraStop.Application.Notifications;

namespace ProcraStop.Infrastructure.Tasks.Repositories
{
    internal class TaskRepository : ITaskRepository, IReliableTaskRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly IDashboardCacheInvalidator _dashboardCache;
        private readonly TimeProvider _timeProvider;

        public TaskRepository(
            IDbContextFactory<AppDbContext> dbContextFactory,
            IDashboardCacheInvalidator dashboardCache,
            TimeProvider? timeProvider = null)
        {
            _dbContextFactory = dbContextFactory;
            _dashboardCache = dashboardCache;
            _timeProvider = timeProvider ?? TimeProvider.System;
        }
        public async Task<StudyTask?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var entity = await dbContext.Tasks
                .Include(x => x.Subtasks)
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

            if (entity is null)
                return null;

            return entity.ToDomain();
        }
        public async Task AddAsync(StudyTask task, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var entity = task.ToEntity();

            await dbContext.Tasks.AddAsync(entity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            _dashboardCache.Invalidate();
        }

        public async Task AddWithReminderIntentsAsync(StudyTask task,
            IReadOnlyList<ReminderSchedule> reminders, CancellationToken ct = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
            await db.Tasks.AddAsync(task.ToEntity(), ct);
            await SyncReminderIntentsAsync(db, task.Id, reminders, ct);
            await db.SaveChangesAsync(ct);
            _dashboardCache.Invalidate();
        }

        public Task UpdateAsync(StudyTask task, CancellationToken cancellationToken = default) =>
            UpdateCoreAsync(task, null, cancellationToken);

        public Task UpdateWithReminderIntentsAsync(StudyTask task,
            IReadOnlyList<ReminderSchedule> reminders, CancellationToken ct = default) =>
            UpdateCoreAsync(task, reminders, ct);

        private async Task UpdateCoreAsync(StudyTask task,
            IReadOnlyList<ReminderSchedule>? reminders, CancellationToken cancellationToken)
        {
            try
            {
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
                Console.WriteLine("========== REPOSITORY UPDATE ==========");

                var existing = await dbContext.Tasks
                    .Include(x => x.Subtasks)
                    .FirstOrDefaultAsync(x => x.Id == task.Id, cancellationToken);

                if (existing is null)
                    return;

                Console.WriteLine($"Existing subtasks DB: {existing.Subtasks.Count}");
                Console.WriteLine($"Incoming subtasks Domain: {task.Subtasks.Count}");

                // =========================
                // UPDATE CAMPOS SIMPLES
                // =========================

                existing.Title = task.Title;
                existing.Description = task.Description;
                existing.DueDate = task.DueDate;
                existing.ReminderDate = task.ReminderDate;
                existing.Priority = (int)task.Priority;
                existing.Status = (int)task.Status;
                existing.IsArchived = task.IsArchived;
                existing.CourseId = task.CourseId;
                existing.EstimatedMinutes = task.EstimatedMinutes;
                existing.CompletedAtUtc = task.CompletedAtUtc;
                existing.ActivityType = (int)task.ActivityType;
                existing.PlannedStartDate = task.PlannedStartDate;
                existing.StartedAtUtc = task.StartedAtUtc;
                existing.FirstStep = task.FirstStep;
                existing.PlannedReviewDate = task.PlannedReviewDate;
                existing.ReviewedAtUtc = task.ReviewedAtUtc;

                // =========================
                // 🔥 SUBTASKS (SOURCE = DOMAIN)
                // =========================

                Console.WriteLine("SYNC SUBTASKS: calculando cambios...");

                // Mejor estrategia: sincronizar cambios explícitamente para evitar que EF
                // genere operaciones UPDATE sobre filas inexistentes (concurrency errors).
                // 1) Eliminar subtareas que ya no vienen
                var incomingIds = task.Subtasks.Select(s => s.Id).ToHashSet();
                var toRemove = existing.Subtasks.Where(s => !incomingIds.Contains(s.Id)).ToList();

                foreach (var rem in toRemove)
                {
                    Console.WriteLine($"REMOVE -> {rem.Id} | {rem.Title}");
                    dbContext.Subtasks.Remove(rem);
                }

                // 2) Actualizar las existentes y añadir las nuevas (usando Add para nuevas)
                foreach (var subtask in task.Subtasks)
                {
                    var found = existing.Subtasks.FirstOrDefault(s => s.Id == subtask.Id);

                    if (found is null)
                    {
                        // Esperamos que la capa de dominio haya generado el Id.
                        if (subtask.Id == Guid.Empty)
                            throw new InvalidOperationException("Subtask Id must be generated by the domain before persisting.");

                        Console.WriteLine($"ADD -> {subtask.Id} | {subtask.Title}");
                        var ent = new SubtaskEntity
                        {
                            Id = subtask.Id,
                            Title = subtask.Title,
                            IsCompleted = subtask.IsCompleted,
                            PlannedDate = subtask.PlannedDate,
                            TaskId = task.Id
                        };

                        // Añadir directamente al DbSet para que EF marque como Added
                        await dbContext.Subtasks.AddAsync(ent, cancellationToken);
                    }
                    else
                    {
                        Console.WriteLine($"UPDATE -> {subtask.Id} | {subtask.Title}");
                        found.Title = subtask.Title;
                        found.IsCompleted = subtask.IsCompleted;
                        found.PlannedDate = subtask.PlannedDate;
                        // TaskId ya está seteado
                    }
                }

                // =========================
                // SAVE
                // =========================

                Console.WriteLine("SAVING CHANGES...");

                foreach (var entry in dbContext.ChangeTracker.Entries<SubtaskEntity>())
                {
                    Console.WriteLine($"TRACKING -> {entry.Entity.Id} | State: {entry.State}");
                }

                if (reminders is not null)
                    await SyncReminderIntentsAsync(dbContext, task.Id, reminders, cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);
                _dashboardCache.Invalidate();

                Console.WriteLine("SAVE OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 ERROR REAL EN REPOSITORY:");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private async Task SyncReminderIntentsAsync(AppDbContext db, Guid taskId,
            IReadOnlyList<ReminderSchedule> reminders, CancellationToken ct)
        {
            var existing = await db.ReminderSchedules.Where(x => x.TaskId == taskId).ToListAsync(ct);
            var desiredKinds = reminders.Select(x => x.Kind).ToHashSet(StringComparer.Ordinal);
            foreach (var stale in existing.Where(x => !desiredKinds.Contains(x.Kind)))
            {
                stale.Status = (int)ReminderScheduleStatus.Cancelled;
                stale.UpdatedAtUtc = _timeProvider.GetUtcNow();
                stale.NextAttemptUtc = null;
            }

            foreach (var reminder in reminders)
            {
                var row = existing.FirstOrDefault(x => x.Kind == reminder.Kind);
                if (row is null)
                {
                    db.ReminderSchedules.Add(new ReminderScheduleEntity
                    {
                        TaskId = reminder.TaskId, Kind = reminder.Kind, Title = reminder.Title,
                        Description = reminder.Description, ScheduledForUtc = reminder.ScheduledForUtc,
                        DueDateUtc = reminder.DueDateUtc, Priority = reminder.Priority,
                        Status = (int)ReminderScheduleStatus.Pending, UpdatedAtUtc = reminder.UpdatedAtUtc
                    });
                    continue;
                }

                row.Title = reminder.Title;
                row.Description = reminder.Description;
                row.ScheduledForUtc = reminder.ScheduledForUtc;
                row.DueDateUtc = reminder.DueDateUtc;
                row.Priority = reminder.Priority;
                row.Status = (int)ReminderScheduleStatus.Pending;
                row.UpdatedAtUtc = reminder.UpdatedAtUtc;
                row.NextAttemptUtc = null;
                row.LastError = null;
            }
        }
    }
}
