namespace ProcraStop.Application.Notifications;

public interface IReminderDiagnosticsService
{
    Task<ReminderCapabilityStatus> GetStatusAsync(CancellationToken ct = default);
    Task<ReminderCapabilityStatus> RequestRequiredAccessAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ReminderHistoryItem>> GetHistoryAsync(int limit = 100, CancellationToken ct = default);
    Task ReconcileAsync(CancellationToken ct = default);
}
