using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Domain.Tasks.Entities;

namespace ProcraStop.Application.Tasks.Services
{
    // editbycodex: centraliza el ciclo de vida de ambos avisos (vencimiento e inicio)
    // para que todos los handlers apliquen la misma regla sin duplicarla.
    public static class TaskReminderSchedulingExtensions
    {
        public static async Task SyncTaskRemindersAsync(
            this ITaskReminderService reminders,
            StudyTask task,
            TimeProvider? timeProvider = null,
            CancellationToken cancellationToken = default)
        {
            timeProvider ??= TimeProvider.System;
            var localNow = timeProvider.GetLocalNow().DateTime;
            if (task.IsArchived || task.IsCompleted)
            {
                await reminders.CancelAsync(task.Id, cancellationToken);
                return;
            }

            if (task.ReminderDate is { } dueReminder && dueReminder > localNow)
            {
                await reminders.ScheduleAsync(
                    task.Id,
                    task.Title,
                    task.Description,
                    dueReminder,
                    task.DueDate,
                    task.Priority,
                    cancellationToken);
            }
            else
            {
                await reminders.CancelDueReminderAsync(task.Id, cancellationToken);
            }

            var startAt = TaskStartReminderPolicy.ResolveNotificationTime(task, localNow);
            if (startAt is { } scheduledStart)
            {
                await reminders.ScheduleStartReminderAsync(
                    task.Id,
                    task.Title,
                    task.Description,
                    scheduledStart,
                    task.Priority,
                    cancellationToken);
            }
            else
            {
                await reminders.CancelStartReminderAsync(task.Id, cancellationToken);
            }
        }
    }
}
