using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcraStop.Infrastructure.DBContext.Migrations;

/// <inheritdoc />
public partial class _20261121031319_InitialNet11 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AcademicHelpRequests",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TaskId = table.Column<Guid>(type: "TEXT", nullable: true),
                CourseId = table.Column<Guid>(type: "TEXT", nullable: true),
                Topic = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Question = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                Channel = table.Column<int>(type: "INTEGER", nullable: false),
                ContactName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                RequestedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                ResolvedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AcademicHelpRequests", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Courses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                Color = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                InterestLevel = table.Column<int>(type: "INTEGER", nullable: true),
                DifficultyLevel = table.Column<int>(type: "INTEGER", nullable: true),
                AppraisedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Courses", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ExamSimulations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ExamTaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                Score = table.Column<int>(type: "INTEGER", nullable: false),
                Errors = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                TakenAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamSimulations", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Habits",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                Unit = table.Column<int>(type: "INTEGER", nullable: false),
                TargetValue = table.Column<int>(type: "INTEGER", nullable: false),
                DaysOfWeek = table.Column<string>(type: "TEXT", nullable: false),
                RequiredPercentage = table.Column<double>(type: "REAL", nullable: false),
                Weight = table.Column<double>(type: "REAL", nullable: false),
                TrackingType = table.Column<int>(type: "INTEGER", nullable: false),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Habits", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ReminderHistory",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                Kind = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                ScheduledForUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                OccurredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                Details = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReminderHistory", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ReminderSchedules",
            columns: table => new
            {
                TaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                Kind = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                ScheduledForUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                DueDateUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                Priority = table.Column<int>(type: "INTEGER", nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                Attempts = table.Column<int>(type: "INTEGER", nullable: false),
                NextAttemptUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                LastError = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReminderSchedules", x => new { x.TaskId, x.Kind });
            });

        migrationBuilder.CreateTable(
            name: "StressRecords",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Level = table.Column<double>(type: "REAL", nullable: false),
                Source = table.Column<int>(type: "INTEGER", nullable: false),
                Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StressRecords", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "StudyTechniqueSessions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TaskId = table.Column<Guid>(type: "TEXT", nullable: true),
                CourseName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                Topic = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Technique = table.Column<int>(type: "INTEGER", nullable: false),
                PlannedMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                CompletedMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                FocusBefore = table.Column<int>(type: "INTEGER", nullable: false),
                EnergyBefore = table.Column<int>(type: "INTEGER", nullable: false),
                ConfidenceBefore = table.Column<int>(type: "INTEGER", nullable: false),
                ConfusionAfter = table.Column<int>(type: "INTEGER", nullable: false),
                ProgressPercent = table.Column<int>(type: "INTEGER", nullable: false),
                PomodoroCycles = table.Column<int>(type: "INTEGER", nullable: false),
                HelpStatus = table.Column<int>(type: "INTEGER", nullable: false),
                StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StudyTechniqueSessions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SystemSnapshots",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                Stress = table.Column<double>(type: "REAL", nullable: false),
                Performance = table.Column<double>(type: "REAL", nullable: false),
                Behavior = table.Column<double>(type: "REAL", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SystemSnapshots", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ClassSchedules",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CourseId = table.Column<Guid>(type: "TEXT", nullable: false),
                DayOfWeek = table.Column<int>(type: "INTEGER", nullable: false),
                StartTicks = table.Column<long>(type: "INTEGER", nullable: false),
                EndTicks = table.Column<long>(type: "INTEGER", nullable: false),
                Classroom = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                Instructor = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                ReminderMinutesBefore = table.Column<int>(type: "INTEGER", nullable: false),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ClassSchedules", x => x.Id);
                table.ForeignKey(
                    name: "FK_ClassSchedules_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Tasks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                ReminderDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                Priority = table.Column<int>(type: "INTEGER", nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                CourseId = table.Column<Guid>(type: "TEXT", nullable: true),
                EstimatedMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                ActivityType = table.Column<int>(type: "INTEGER", nullable: false),
                PlannedStartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                FirstStep = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                PlannedReviewDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                ReviewedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tasks", x => x.Id);
                table.ForeignKey(
                    name: "FK_Tasks_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "HabitLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                HabitId = table.Column<Guid>(type: "TEXT", nullable: false),
                Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                Value = table.Column<int>(type: "INTEGER", nullable: false),
                Notes = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HabitLogs", x => x.Id);
                table.ForeignKey(
                    name: "FK_HabitLogs_Habits_HabitId",
                    column: x => x.HabitId,
                    principalTable: "Habits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Subtasks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                PlannedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Subtasks", x => x.Id);
                table.ForeignKey(
                    name: "FK_Subtasks_Tasks_TaskId",
                    column: x => x.TaskId,
                    principalTable: "Tasks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AcademicHelpRequests_Status_CreatedAtUtc",
            table: "AcademicHelpRequests",
            columns: new[] { "Status", "CreatedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_AcademicHelpRequests_TaskId",
            table: "AcademicHelpRequests",
            column: "TaskId");

        migrationBuilder.CreateIndex(
            name: "IX_ClassSchedules_CourseId_DayOfWeek_StartTicks",
            table: "ClassSchedules",
            columns: new[] { "CourseId", "DayOfWeek", "StartTicks" });

        migrationBuilder.CreateIndex(
            name: "IX_ClassSchedules_IsActive_DayOfWeek_StartTicks",
            table: "ClassSchedules",
            columns: new[] { "IsActive", "DayOfWeek", "StartTicks" });

        migrationBuilder.CreateIndex(
            name: "IX_ExamSimulations_ExamTaskId_TakenAtUtc",
            table: "ExamSimulations",
            columns: new[] { "ExamTaskId", "TakenAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_HabitLogs_Date",
            table: "HabitLogs",
            column: "Date");

        migrationBuilder.CreateIndex(
            name: "IX_HabitLogs_HabitId_Date",
            table: "HabitLogs",
            columns: new[] { "HabitId", "Date" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Habits_IsActive_Name",
            table: "Habits",
            columns: new[] { "IsActive", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_ReminderHistory_OccurredAtUtc",
            table: "ReminderHistory",
            column: "OccurredAtUtc");

        migrationBuilder.CreateIndex(
            name: "IX_ReminderHistory_TaskId_OccurredAtUtc",
            table: "ReminderHistory",
            columns: new[] { "TaskId", "OccurredAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_ReminderSchedules_Status_ScheduledForUtc",
            table: "ReminderSchedules",
            columns: new[] { "Status", "ScheduledForUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_StressRecords_Source_Timestamp",
            table: "StressRecords",
            columns: new[] { "Source", "Timestamp" });

        migrationBuilder.CreateIndex(
            name: "IX_StressRecords_Timestamp",
            table: "StressRecords",
            column: "Timestamp");

        migrationBuilder.CreateIndex(
            name: "IX_StudyTechniqueSessions_StartedAtUtc",
            table: "StudyTechniqueSessions",
            column: "StartedAtUtc");

        migrationBuilder.CreateIndex(
            name: "IX_StudyTechniqueSessions_TaskId",
            table: "StudyTechniqueSessions",
            column: "TaskId");

        migrationBuilder.CreateIndex(
            name: "IX_Subtasks_TaskId",
            table: "Subtasks",
            column: "TaskId");

        migrationBuilder.CreateIndex(
            name: "IX_SystemSnapshots_Timestamp",
            table: "SystemSnapshots",
            column: "Timestamp");

        migrationBuilder.CreateIndex(
            name: "IX_Tasks_CourseId",
            table: "Tasks",
            column: "CourseId");

        migrationBuilder.CreateIndex(
            name: "IX_Tasks_IsArchived_CreatedAtUtc",
            table: "Tasks",
            columns: new[] { "IsArchived", "CreatedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_Tasks_IsArchived_DueDate",
            table: "Tasks",
            columns: new[] { "IsArchived", "DueDate" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AcademicHelpRequests");

        migrationBuilder.DropTable(
            name: "ClassSchedules");

        migrationBuilder.DropTable(
            name: "ExamSimulations");

        migrationBuilder.DropTable(
            name: "HabitLogs");

        migrationBuilder.DropTable(
            name: "ReminderHistory");

        migrationBuilder.DropTable(
            name: "ReminderSchedules");

        migrationBuilder.DropTable(
            name: "StressRecords");

        migrationBuilder.DropTable(
            name: "StudyTechniqueSessions");

        migrationBuilder.DropTable(
            name: "Subtasks");

        migrationBuilder.DropTable(
            name: "SystemSnapshots");

        migrationBuilder.DropTable(
            name: "Habits");

        migrationBuilder.DropTable(
            name: "Tasks");

        migrationBuilder.DropTable(
            name: "Courses");
    }
}
