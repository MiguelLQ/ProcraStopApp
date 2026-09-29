using ProcraStop.Application.Tasks.Services;
using ProcraStop.Domain.Tasks.Entities;
using ProcraStop.Domain.Tasks.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Infrastructure.Tasks.Persistence.Mappings;

namespace ProcraStop.UnitTests;

public sealed class AcademicActivityTests
{
    [Fact]
    public void Ordinary_task_keeps_simple_completion()
    {
        var task = NewTask(DateTime.Today.AddDays(2));
        task.Start();
        task.Complete();

        Assert.Equal(TaskItemStatus.Completed, task.Status);
        Assert.NotNull(task.StartedAtUtc);
        Assert.NotNull(task.CompletedAtUtc);
    }

    [Fact]
    public void Dated_milestone_records_progress_and_start()
    {
        var task = NewTask(DateTime.Today.AddDays(5));
        var milestoneDate = DateTime.Today.AddDays(2);
        task.AddSubtask("Borrador", plannedDate: milestoneDate);

        task.ToggleSubtask(task.Subtasks.Single().Id);

        Assert.Equal(milestoneDate, task.Subtasks.Single().PlannedDate);
        Assert.NotNull(task.StartedAtUtc);
        Assert.Equal(TaskItemStatus.Completed, task.Status);
    }

    [Fact]
    public void Milestone_cannot_be_planned_after_deadline()
    {
        var task = NewTask(DateTime.Today.AddDays(2));
        Assert.Throws<InvalidOperationException>(() =>
            task.AddSubtask("Borrador", plannedDate: DateTime.Today.AddDays(3)));
    }

    [Fact]
    public void Deliverable_requires_review_after_milestones()
    {
        var task = NewTask(DateTime.Today.AddDays(5));
        task.SetAcademicPlan(AcademicActivityType.Deliverable, DateTime.Today, "Abrir borrador", DateTime.Today.AddDays(4));
        task.AddSubtask("Redactar borrador");

        Assert.Throws<InvalidOperationException>(task.MarkReviewed);
        task.ToggleSubtask(task.Subtasks.Single().Id);
        Assert.Equal(TaskItemStatus.InProgress, task.Status);
        Assert.Throws<InvalidOperationException>(task.Complete);

        task.MarkReviewed();
        task.Complete();

        Assert.NotNull(task.ReviewedAtUtc);
        Assert.Equal(TaskItemStatus.Completed, task.Status);
    }

    [Fact]
    public void Exam_plan_places_topics_and_retrieval_before_exam()
    {
        var start = new DateTime(2026, 10, 1);
        var exam = new DateTime(2026, 10, 10);

        var plan = ExamPlanBuilder.Build(["Álgebra", "Cálculo"], start, exam);

        Assert.Equal(4, plan.Count);
        Assert.All(plan, step => Assert.InRange(step.PlannedDate!.Value, start, exam.AddDays(-1)));
        Assert.Contains(plan, step => step.Title.Contains("sin apuntes"));
        Assert.Contains(plan, step => step.Title.Contains("Simulacro"));
    }

    [Fact]
    public void Academic_plan_survives_sqlite_round_trip()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        var due = new DateTime(2026, 10, 20, 18, 0, 0);
        var task = NewTask(due);
        task.SetAcademicPlan(AcademicActivityType.Deliverable, due.AddDays(-5).Date,
            "Abrir el documento", due.AddDays(-1).Date);
        task.AddSubtask("Borrador", plannedDate: due.AddDays(-3).Date);

        using (var db = new AppDbContext(options))
        {
            db.Database.EnsureCreated();
            db.Tasks.Add(task.ToEntity());
            db.SaveChanges();
        }

        using (var db = new AppDbContext(options))
        {
            var restored = db.Tasks.Include(x => x.Subtasks).Single().ToDomain();
            Assert.Equal(AcademicActivityType.Deliverable, restored.ActivityType);
            Assert.Equal("Abrir el documento", restored.FirstStep);
            Assert.Equal(due.AddDays(-5).Date, restored.PlannedStartDate);
            Assert.Equal(due.AddDays(-1).Date, restored.PlannedReviewDate);
            Assert.Equal(due.AddDays(-3).Date, restored.Subtasks.Single().PlannedDate);
        }
    }

    private static StudyTask NewTask(DateTime dueDate) =>
        StudyTask.Create("Actividad", null, dueDate, TaskPriority.Medium, null);
}
