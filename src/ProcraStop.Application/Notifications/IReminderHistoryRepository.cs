namespace ProcraStop.Application.Notifications;

public interface IReminderHistoryRepository
{
    Task AddAsync(ReminderHistoryItem item, CancellationToken ct = default);
    Task<IReadOnlyList<ReminderHistoryItem>> GetRecentAsync(int limit = 100, CancellationToken ct = default);
    Task<IReadOnlyList<ReminderHistoryItem>> GetForTaskAsync(Guid taskId, CancellationToken ct = default);
}
