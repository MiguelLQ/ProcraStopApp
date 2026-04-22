using Microsoft.EntityFrameworkCore;
using ProcraStop.Modules.Habits.Infraestructure.Persistence.Entities;
using ProcraStop.Modules.Stress.Infrastructure.Persistences.Config;
using ProcraStop.Modules.Stress.Infrastructure.Persistences.Entities;

namespace ProcraStop.Modules.Tasks.Infraestructure.Persistence
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
        // DbSet para las subtareas
        public DbSet<SubtaskEntity> Subtasks => Set<SubtaskEntity>();
        // para habits
        public DbSet<HabitEntity> Habits => Set<HabitEntity>();
        public DbSet<HabitLogEntity> HabitLogs => Set<HabitLogEntity>();
        // streess
        public DbSet<StressRecordEntity> StressRecords => Set<StressRecordEntity>();

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

                entity.Property(x => x.CreatedAtUtc)
                    .IsRequired();
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
            });

            modelBuilder.Entity<HabitLogEntity>(entity =>
            {
                entity.ToTable("HabitLogs");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Date).IsRequired();

                entity.Property(x => x.Value).IsRequired();

                entity.HasOne<HabitEntity>()
                    .WithMany()
                    .HasForeignKey(x => x.HabitId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // estress
            
            modelBuilder.Entity<StudyTaskEntity>()
            .HasOne(t => t.Course)
            .WithMany()
            .HasForeignKey(t => t.CourseId);
            // navegacion habit logs
           
        }

    }
}
