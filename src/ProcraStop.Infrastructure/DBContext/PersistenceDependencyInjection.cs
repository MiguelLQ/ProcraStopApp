
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ProcraStop.Infrastructure.DBContext
{
    public static class PersistenceDependencyInjection
    {
        public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string databasePath)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={databasePath}"));

            return services;
        }

        public static void InitializeDatabase(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.EnsureCreated();
            db.Database.ExecuteSqlRaw("""
                CREATE TABLE IF NOT EXISTS ClassSchedules (
                    Id TEXT NOT NULL CONSTRAINT PK_ClassSchedules PRIMARY KEY,
                    CourseId TEXT NOT NULL,
                    DayOfWeek INTEGER NOT NULL,
                    StartTicks INTEGER NOT NULL,
                    EndTicks INTEGER NOT NULL,
                    Classroom TEXT NULL,
                    Instructor TEXT NULL,
                    ReminderMinutesBefore INTEGER NOT NULL,
                    IsActive INTEGER NOT NULL,
                    CreatedAtUtc TEXT NOT NULL,
                    CONSTRAINT FK_ClassSchedules_Courses_CourseId FOREIGN KEY (CourseId) REFERENCES Courses (Id) ON DELETE CASCADE
                );
                """);
            db.Database.ExecuteSqlRaw("""
                CREATE INDEX IF NOT EXISTS IX_ClassSchedules_CourseId_DayOfWeek_StartTicks
                ON ClassSchedules (CourseId, DayOfWeek, StartTicks);
                """);
        }
    }
}
