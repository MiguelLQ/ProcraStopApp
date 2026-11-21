using Microsoft.EntityFrameworkCore;
using ProcraStop.Application.Notifications;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Infrastructure.Tasks.Persistence.Entities;

namespace ProcraStop.Infrastructure.Tasks.Repositories;

public sealed class ReminderScheduleRepository : IReminderScheduleRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly TimeProvider _timeProvider;
    public ReminderScheduleRepository(IDbContextFactory<AppDbContext> factory, TimeProvider? timeProvider = null)
    {
        _factory = factory;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task UpsertAsync(ReminderSchedule schedule, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var entity = await db.ReminderSchedules.FindAsync([schedule.TaskId, schedule.Kind], ct);
        if (entity is null)
            db.ReminderSchedules.Add(ToEntity(schedule));
        else
        {
            entity.Title = schedule.Title;
            entity.Description = schedule.Description;
            entity.ScheduledForUtc = schedule.ScheduledForUtc;
            entity.DueDateUtc = schedule.DueDateUtc;
            entity.Priority = schedule.Priority;
            entity.Status = (int)schedule.Status;
            entity.UpdatedAtUtc = schedule.UpdatedAtUtc;
            entity.Attempts = schedule.Attempts;
            entity.NextAttemptUtc = schedule.NextAttemptUtc;
            entity.LastError = schedule.LastError;
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task<ReminderSchedule?> GetAsync(Guid taskId, string kind, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var row = await db.ReminderSchedules.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TaskId == taskId && x.Kind == kind, ct);
        return row is null ? null : ToModel(row);
    }

    public async Task<IReadOnlyList<ReminderSchedule>> GetActiveAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var rows = await db.ReminderSchedules.AsNoTracking()
            .Where(x => x.Status == (int)ReminderScheduleStatus.Active ||
                        x.Status == (int)ReminderScheduleStatus.Pending)
            .OrderBy(x => x.ScheduledForUtc)
            .ToListAsync(ct);
        return rows.Select(ToModel).ToList();
    }

    public async Task SetStatusAsync(Guid taskId, string kind, ReminderScheduleStatus status, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var entity = await db.ReminderSchedules.FindAsync([taskId, kind], ct);
        if (entity is null) return;
        entity.Status = (int)status;
        entity.UpdatedAtUtc = _timeProvider.GetUtcNow();
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkAttemptAsync(Guid taskId, string kind, ReminderScheduleStatus status,
        DateTimeOffset? nextAttemptUtc, string? error, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var entity = await db.ReminderSchedules.FindAsync([taskId, kind], ct);
        if (entity is null) return;
        entity.Status = (int)status;
        entity.Attempts++;
        entity.NextAttemptUtc = nextAttemptUtc;
        entity.LastError = error;
        entity.UpdatedAtUtc = _timeProvider.GetUtcNow();
        await db.SaveChangesAsync(ct);
    }

    private static ReminderScheduleEntity ToEntity(ReminderSchedule x) => new()
    {
        TaskId = x.TaskId, Kind = x.Kind, Title = x.Title, Description = x.Description,
        ScheduledForUtc = x.ScheduledForUtc, DueDateUtc = x.DueDateUtc,
        Priority = x.Priority, Status = (int)x.Status, UpdatedAtUtc = x.UpdatedAtUtc,
        Attempts = x.Attempts, NextAttemptUtc = x.NextAttemptUtc, LastError = x.LastError
    };

    private static ReminderSchedule ToModel(ReminderScheduleEntity x) => new(
        x.TaskId, x.Kind, x.Title, x.Description, x.ScheduledForUtc, x.DueDateUtc,
        x.Priority, (ReminderScheduleStatus)x.Status, x.UpdatedAtUtc)
        { Attempts = x.Attempts, NextAttemptUtc = x.NextAttemptUtc, LastError = x.LastError };
}
