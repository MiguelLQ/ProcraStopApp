using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.Commands.ArchiveTask;
using ProcraStop.Application.Tasks.Commands.CompleteTask;
using ProcraStop.Application.Tasks.Commands.CreateTask;
using ProcraStop.Application.Tasks.Commands.StartTask;
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

    [Fact]
    public async Task Starting_task_cancels_start_reminder_but_keeps_due_reminder()
    {
        var task = NewTask();
        task.SetAcademicPlan(AcademicActivityType.Task, DateTime.Today.AddDays(-1), null, null);
        var reminders = new FakeReminders();

        await new StartTaskCommandHandler(new FakeTasks(task), reminders).HandleAsync(task.Id);

        Assert.Equal(TaskItemStatus.InProgress, task.Status);
        Assert.Equal(task.Id, reminders.StartReminderCancelled);
        Assert.Null(reminders.Cancelled);
        Assert.Null(reminders.DueReminderCancelled);
    }

    [Fact]
    public async Task Creating_task_with_overdue_planned_start_schedules_start_reminder()
    {
        var tasks = new FakeTasks(null);
        var reminders = new FakeReminders();

        var taskId = await new CreateTaskCommandHandler(tasks, reminders).HandleAsync(new CreateTaskCommand
        {
            Title = "Tarea D1",
            DueDate = DateTime.Today.AddDays(3),
            PlannedStartDate = DateTime.Today.AddDays(-1)
        });

        Assert.Equal(taskId, reminders.StartReminderScheduled);
        Assert.NotNull(reminders.StartReminderDate);
        Assert.True(reminders.StartReminderDate <= DateTime.Now.AddMinutes(3));
    }

    [Fact]
    public async Task Creating_task_without_planned_start_does_not_schedule_start_reminder()
    {
        var tasks = new FakeTasks(null);
        var reminders = new FakeReminders();

        await new CreateTaskCommandHandler(tasks, reminders).HandleAsync(new CreateTaskCommand
        {
            Title = "Tarea sin plan",
            DueDate = DateTime.Today.AddDays(3)
        });

        Assert.Null(reminders.StartReminderScheduled);
    }

    private static StudyTask NewTask() => StudyTask.Create("Tarea", null,
        DateTime.Now.AddDays(1), TaskPriority.Medium, null);

    private sealed class FakeTasks(StudyTask? task) : ITaskRepository
    {
        private StudyTask? _task = task;

        public Task<StudyTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_task is not null && id == _task.Id ? _task : null);
        public Task AddAsync(StudyTask item, CancellationToken cancellationToken = default)
        {
            _task = item;
            return Task.CompletedTask;
        }
        public Task UpdateAsync(StudyTask item, CancellationToken cancellationToken = default)
        {
            _task = item;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeReminders : ITaskReminderService
    {
        public Guid? Cancelled { get; private set; }
        public Guid? DueReminderCancelled { get; private set; }
        public Guid? StartReminderCancelled { get; private set; }
        public Guid? DueReminderScheduled { get; private set; }
        public Guid? StartReminderScheduled { get; private set; }
        public DateTime? StartReminderDate { get; private set; }

        public Task ScheduleAsync(Guid taskId, string title, string? description, DateTime reminderDate,
            DateTime? dueDate, TaskPriority priority, CancellationToken cancellationToken = default)
        {
            DueReminderScheduled = taskId;
            return Task.CompletedTask;
        }

        public Task ScheduleStartReminderAsync(Guid taskId, string title, string? description,
            DateTime plannedStartDate, TaskPriority priority, CancellationToken cancellationToken = default)
        {
            StartReminderScheduled = taskId;
            StartReminderDate = plannedStartDate;
            return Task.CompletedTask;
        }

        public Task CancelDueReminderAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            DueReminderCancelled = taskId;
            return Task.CompletedTask;
        }

        public Task CancelStartReminderAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            StartReminderCancelled = taskId;
            return Task.CompletedTask;
        }

        public Task CancelAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            Cancelled = taskId;
            return Task.CompletedTask;
        }
    }
}
