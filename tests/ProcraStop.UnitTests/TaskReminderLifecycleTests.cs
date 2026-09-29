using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.Commands.ArchiveTask;
using ProcraStop.Application.Tasks.Commands.CompleteTask;
using ProcraStop.Application.Tasks.Commands.ToggleSubtask;
using ProcraStop.Domain.Tasks.Entities;
using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.UnitTests;

public sealed class TaskReminderLifecycleTests
{
    [Fact]
    public async Task Completing_task_cancels_pending_reminder()
    {
        var task = NewTask();
        var reminders = new FakeReminders();
        await new CompleteTaskCommandHandler(new FakeTasks(task), reminders)
            .HandleAsync(new CompleteTaskCommand { TaskId = task.Id });

        Assert.True(task.IsCompleted);
        Assert.Equal(task.Id, reminders.Cancelled);
    }

    [Fact]
    public async Task Archiving_task_cancels_pending_reminder()
    {
        var task = NewTask();
        var reminders = new FakeReminders();
        await new ArchiveTaskCommandHandler(new FakeTasks(task), reminders)
            .HandleAsync(new ArchiveTaskCommand { TaskId = task.Id }, CancellationToken.None);

        Assert.True(task.IsArchived);
        Assert.Equal(task.Id, reminders.Cancelled);
    }

    [Fact]
    public async Task Finishing_last_subtask_cancels_pending_reminder()
    {
        var task = NewTask();
        task.AddSubtask("Hito");
        var reminders = new FakeReminders();
        await new ToggleSubtaskCommandHandler(new FakeTasks(task), reminders)
            .HandleAsync(new ToggleSubtaskCommand { TaskId = task.Id, SubtaskId = task.Subtasks.Single().Id },
                CancellationToken.None);

        Assert.True(task.IsCompleted);
        Assert.Equal(task.Id, reminders.Cancelled);
    }

    private static StudyTask NewTask() => StudyTask.Create("Tarea", null,
        DateTime.Now.AddDays(1), TaskPriority.Medium, null);

    private sealed class FakeTasks(StudyTask task) : ITaskRepository
    {
        public Task<StudyTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<StudyTask?>(id == task.Id ? task : null);
        public Task AddAsync(StudyTask item, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(StudyTask item, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeReminders : ITaskReminderService
    {
        public Guid? Cancelled { get; private set; }
        public Task ScheduleAsync(Guid taskId, string title, string? description, DateTime reminderDate,
            DateTime? dueDate, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CancelAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            Cancelled = taskId;
            return Task.CompletedTask;
        }
    }
}
