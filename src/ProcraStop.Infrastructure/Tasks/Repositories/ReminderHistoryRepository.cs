using Microsoft.EntityFrameworkCore;
using ProcraStop.Application.Notifications;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Infrastructure.Tasks.Persistence.Entities;

namespace ProcraStop.Infrastructure.Tasks.Repositories;

public sealed class ReminderHistoryRepository : IReminderHistoryRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public ReminderHistoryRepository(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task AddAsync(ReminderHistoryItem item, CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        db.ReminderHistory.Add(ToEntity(item));
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ReminderHistoryItem>> GetRecentAsync(
        int limit = 100,
        CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 500);
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        var rows = await db.ReminderHistory.AsNoTracking()
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(limit)
            .ToListAsync(ct);
        return rows.Select(ToModel).ToList();
    }

    public async Task<IReadOnlyList<ReminderHistoryItem>> GetForTaskAsync(
        Guid taskId,
        CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        var rows = await db.ReminderHistory.AsNoTracking()
            .Where(x => x.TaskId == taskId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .ToListAsync(ct);
        return rows.Select(ToModel).ToList();
    }

    private static ReminderHistoryEntity ToEntity(ReminderHistoryItem item) => new()
    {
        Id = item.Id,
        TaskId = item.TaskId,
        Kind = item.Kind,
        Status = (int)item.Status,
        Title = item.Title,
        ScheduledForUtc = item.ScheduledForUtc,
        OccurredAtUtc = item.OccurredAtUtc,
        Details = item.Details
    };

    private static ReminderHistoryItem ToModel(ReminderHistoryEntity item) => new(
        item.Id,
        item.TaskId,
        item.Kind,
        (ReminderEventStatus)item.Status,
        item.Title,
        item.ScheduledForUtc,
        item.OccurredAtUtc,
        item.Details);
}
