using ProcraStop.Application.Notifications;
using ProcraStop.Domain.Tasks.Entities;

namespace ProcraStop.Application.Tasks.Abstractions;

public interface IReliableTaskRepository
{
    Task AddWithReminderIntentsAsync(StudyTask task, IReadOnlyList<ReminderSchedule> reminders,
        CancellationToken ct = default);
    Task UpdateWithReminderIntentsAsync(StudyTask task, IReadOnlyList<ReminderSchedule> reminders,
        CancellationToken ct = default);
}
