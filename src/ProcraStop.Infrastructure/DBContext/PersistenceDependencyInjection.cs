
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
            services.AddSingleton<DatabasePerformanceInterceptor>();
            services.AddDbContextFactory<AppDbContext>((provider, options) =>
                options
                    .UseSqlite($"Data Source={databasePath}")
                    .AddInterceptors(provider.GetRequiredService<DatabasePerformanceInterceptor>()));

            return services;
        }

        public static void InitializeDatabase(IServiceProvider services)
        {
            var factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var db = factory.CreateDbContext();

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
            db.Database.ExecuteSqlRaw("""
                CREATE TABLE IF NOT EXISTS StudyTechniqueSessions (
                    Id TEXT NOT NULL CONSTRAINT PK_StudyTechniqueSessions PRIMARY KEY,
                    CourseName TEXT NOT NULL,
                    Topic TEXT NOT NULL,
                    Technique INTEGER NOT NULL,
                    PlannedMinutes INTEGER NOT NULL,
                    CompletedMinutes INTEGER NOT NULL,
                    FocusBefore INTEGER NOT NULL,
                    EnergyBefore INTEGER NOT NULL,
                    ConfidenceBefore INTEGER NOT NULL,
                    ConfusionAfter INTEGER NOT NULL,
                    ProgressPercent INTEGER NOT NULL,
                    PomodoroCycles INTEGER NOT NULL,
                    HelpStatus INTEGER NOT NULL,
                    StartedAtUtc TEXT NOT NULL,
                    CompletedAtUtc TEXT NULL,
                    Notes TEXT NULL
                );
                """);
            db.Database.ExecuteSqlRaw("""
                CREATE INDEX IF NOT EXISTS IX_StudyTechniqueSessions_StartedAtUtc
                ON StudyTechniqueSessions (StartedAtUtc);
                """);
        }
    }
}
