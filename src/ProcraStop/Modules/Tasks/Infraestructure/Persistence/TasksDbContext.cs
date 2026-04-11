using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcraStop.Modules.Tasks.Infraestructure.Persistence
{
    public sealed class TasksDbContext :DbContext
    {
       
        public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options)
        {

        }

        public DbSet<StudyTaskEntity> Tasks => Set<StudyTaskEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
                entity.Property(x => x.CourseId);
                entity.Property(x => x.EstimatedMinutes);
                entity.Property(x => x.CompletedAtUtc);
            });
        }

    }
}
