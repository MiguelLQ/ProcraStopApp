using Microsoft.EntityFrameworkCore;

namespace ProcraStop.Modules.Tasks.Infraestructure.Persistence
{
    public sealed class TasksDbContext :DbContext
    {
       
        public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options)
        {

        }
        // DbSet para las tareas
        public DbSet<StudyTaskEntity> Tasks => Set<StudyTaskEntity>();
        // DbSet para los cursos
        public DbSet<CourseEntity> Courses => Set<CourseEntity>();
        // DbSet para las subtareas
        public DbSet<SubtaskEntity> Subtasks => Set<SubtaskEntity>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
        }

    }
}
