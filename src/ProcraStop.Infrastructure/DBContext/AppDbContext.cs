

using Microsoft.EntityFrameworkCore;
using ProcraStop.Infrastructure.Academic.Persistence.Entities;
using ProcraStop.Infrastructure.Dashboard.Persistence.Entities;
using ProcraStop.Infrastructure.Habits.Persistence.Entities;
using ProcraStop.Infrastructure.Stress.Persistence.Configs;
using ProcraStop.Infrastructure.Stress.Persistence.Entities;
using ProcraStop.Infrastructure.StudyTechniques.Persistence.Entities;
using ProcraStop.Infrastructure.Tasks.Persistence.Entities;

namespace ProcraStop.Infrastructure.DBContext
{
    public sealed class AppDbContext :DbContext
    {
       
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        // DbSet para las tareas
        public DbSet<StudyTaskEntity> Tasks => Set<StudyTaskEntity>();
        // DbSet para los cursos
        public DbSet<CourseEntity> Courses => Set<CourseEntity>();
        public DbSet<ClassScheduleEntity> ClassSchedules => Set<ClassScheduleEntity>();
        // DbSet para las subtareas
        public DbSet<SubtaskEntity> Subtasks => Set<SubtaskEntity>();
        // para habits
        public DbSet<HabitEntity> Habits => Set<HabitEntity>();
        public DbSet<HabitLogEntity> HabitLogs => Set<HabitLogEntity>();
        // streess
        public DbSet<StressRecordEntity> StressRecords => Set<StressRecordEntity>();
        public DbSet<StudyTechniqueSessionEntity> StudyTechniqueSessions => Set<StudyTechniqueSessionEntity>();
        public DbSet<ExamSimulationEntity> ExamSimulations => Set<ExamSimulationEntity>();
        public DbSet<AcademicHelpRequestEntity> AcademicHelpRequests => Set<AcademicHelpRequestEntity>();
        // memoria del sistema
        public DbSet<SystemSnapshotEntity> SystemSnapshots => Set<SystemSnapshotEntity>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // IMPORTANTE: carga configs externas (incluye Stress)
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(StressRecordConfig).Assembly
            );

            // Configuración de la entidad StudyTaskEntity
            modelBuilder.Entity<StudyTaskEntity>(entity =>
            {
                entity.ToTable("Tasks");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Description)
                    .HasMaxLength(1000);

                entity.Property(x => x.Priority)
                    .IsRequired();

                entity.Property(x => x.Status)
                    .IsRequired();

                entity.Property(x => x.IsArchived)
                    .IsRequired();

                entity.Property(x => x.CreatedAtUtc)
                    .IsRequired();
                entity.Property(x => x.DueDate);
                entity.Property(x => x.ReminderDate);
                // Configuración de la relación con CourseEntity
                entity.HasOne<CourseEntity>()
                    .WithMany()
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(x => x.EstimatedMinutes);
                entity.Property(x => x.CompletedAtUtc);
                entity.Property(x => x.ActivityType).IsRequired();
                entity.Property(x => x.PlannedStartDate);
                entity.Property(x => x.StartedAtUtc);
                entity.Property(x => x.FirstStep).HasMaxLength(300);
                entity.Property(x => x.PlannedReviewDate);
                entity.Property(x => x.ReviewedAtUtc);

                entity.HasIndex(x => new { x.IsArchived, x.DueDate })
                    .HasDatabaseName("IX_Tasks_IsArchived_DueDate");
                entity.HasIndex(x => new { x.IsArchived, x.CreatedAtUtc })
                    .HasDatabaseName("IX_Tasks_IsArchived_CreatedAtUtc");
            });
            // Configuración de la entidad CourseEntity
            modelBuilder.Entity<CourseEntity>(entity =>
            {
                entity.ToTable("Courses");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.Property(x => x.Color)
                    .HasMaxLength(20);

                entity.Property(x => x.InterestLevel);
                entity.Property(x => x.DifficultyLevel);
                entity.Property(x => x.AppraisedAtUtc);

                entity.Property(x => x.CreatedAtUtc)
                    .IsRequired();
            });
            modelBuilder.Entity<ClassScheduleEntity>(entity =>
            {
                entity.ToTable("ClassSchedules");

                entity.HasKey(x => x.Id);
                entity.Property(x => x.CourseId).IsRequired();
                entity.Property(x => x.DayOfWeek).IsRequired();
                entity.Property(x => x.StartTicks).IsRequired();
                entity.Property(x => x.EndTicks).IsRequired();
                entity.Property(x => x.Classroom).HasMaxLength(120);
                entity.Property(x => x.Instructor).HasMaxLength(120);
                entity.Property(x => x.ReminderMinutesBefore).IsRequired();
                entity.Property(x => x.IsActive).IsRequired();
                entity.Property(x => x.CreatedAtUtc).IsRequired();

                entity.HasIndex(x => new { x.CourseId, x.DayOfWeek, x.StartTicks });
                entity.HasIndex(x => new { x.IsActive, x.DayOfWeek, x.StartTicks })
                    .HasDatabaseName("IX_ClassSchedules_IsActive_DayOfWeek_StartTicks");
                entity.HasOne(x => x.Course)
                    .WithMany()
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // Configuración de la entidad SubtaskEntity
            modelBuilder.Entity<SubtaskEntity>(entity =>
            {
                entity.ToTable("Subtasks");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.IsCompleted)
                    .IsRequired();
                entity.Property(x => x.PlannedDate);

                entity.HasOne(x => x.Task)
                    .WithMany(t => t.Subtasks)
                    .HasForeignKey(x => x.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // habits
            modelBuilder.Entity<HabitEntity>(entity =>
            {
                entity.ToTable("Habits");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.Unit).IsRequired();

                entity.Property(x => x.TargetValue).IsRequired();

                entity.Property(x => x.DaysOfWeek).IsRequired();

                entity.Property(x => x.RequiredPercentage).IsRequired();

                entity.Property(x => x.IsActive).IsRequired();
                entity.Property(x => x.Weight).IsRequired();
                entity.Property(x => x.TrackingType).IsRequired();

                entity.HasIndex(x => new { x.IsActive, x.Name })
                    .HasDatabaseName("IX_Habits_IsActive_Name");
            });

            modelBuilder.Entity<HabitLogEntity>(entity =>
            {
                entity.ToTable("HabitLogs");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Date).IsRequired();

                entity.Property(x => x.Value).IsRequired();

                entity.HasIndex(x => new { x.HabitId, x.Date })
                    .IsUnique(); // editbycodex: garantiza un registro diario por hábito en bases nuevas.
                entity.HasIndex(x => x.Date)
                    .HasDatabaseName("IX_HabitLogs_Date");

                entity.HasOne<HabitEntity>()
                    .WithMany()
                    .HasForeignKey(x => x.HabitId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // memoria
            modelBuilder.Entity<SystemSnapshotEntity>(entity =>
            {
                entity.ToTable("SystemSnapshots");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Timestamp)
                    .IsRequired();

                entity.Property(x => x.Stress).IsRequired();
                entity.Property(x => x.Performance).IsRequired();
                entity.Property(x => x.Behavior).IsRequired();

                entity.HasIndex(x => x.Timestamp)
                    .HasDatabaseName("IX_SystemSnapshots_Timestamp");
            });

            modelBuilder.Entity<StudyTechniqueSessionEntity>(entity =>
            {
                entity.ToTable("StudyTechniqueSessions");

                entity.HasKey(x => x.Id);
                entity.Property(x => x.TaskId);
                entity.Property(x => x.CourseName).IsRequired().HasMaxLength(150);
                entity.Property(x => x.Topic).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Technique).IsRequired();
                entity.Property(x => x.PlannedMinutes).IsRequired();
                entity.Property(x => x.CompletedMinutes).IsRequired();
                entity.Property(x => x.FocusBefore).IsRequired();
                entity.Property(x => x.EnergyBefore).IsRequired();
                entity.Property(x => x.ConfidenceBefore).IsRequired();
                entity.Property(x => x.ConfusionAfter).IsRequired();
                entity.Property(x => x.ProgressPercent).IsRequired();
                entity.Property(x => x.PomodoroCycles).IsRequired();
                entity.Property(x => x.HelpStatus).IsRequired();
                entity.Property(x => x.StartedAtUtc).IsRequired();
                entity.Property(x => x.Notes).HasMaxLength(1000);
                entity.HasIndex(x => x.StartedAtUtc);
                entity.HasIndex(x => x.TaskId);
            });
            modelBuilder.Entity<ExamSimulationEntity>(entity =>
            {
                entity.ToTable("ExamSimulations");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ExamTaskId).IsRequired();
                entity.Property(x => x.Score).IsRequired();
                entity.Property(x => x.Errors).HasMaxLength(1000);
                entity.Property(x => x.TakenAtUtc).IsRequired();
                entity.HasIndex(x => new { x.ExamTaskId, x.TakenAtUtc });
            });
            modelBuilder.Entity<AcademicHelpRequestEntity>(entity =>
            {
                entity.ToTable("AcademicHelpRequests");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Topic).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Question).IsRequired().HasMaxLength(2000);
                entity.Property(x => x.ContactName).HasMaxLength(150);
                entity.Property(x => x.Channel).IsRequired();
                entity.Property(x => x.Status).IsRequired();
                entity.Property(x => x.CreatedAtUtc).IsRequired();
                entity.HasIndex(x => new { x.Status, x.CreatedAtUtc });
                entity.HasIndex(x => x.TaskId);
            });

            modelBuilder.Entity<StudyTaskEntity>()
            .HasOne(t => t.Course)
            .WithMany()
            .HasForeignKey(t => t.CourseId);
            // navegacion habit logs
           
        }

    }
}
