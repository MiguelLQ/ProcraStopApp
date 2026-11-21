
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data;

namespace ProcraStop.Infrastructure.DBContext
{
    public static class PersistenceDependencyInjection
    {
        public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string databasePath)
        {
            services.TryAddSingleton(TimeProvider.System);
            services.AddSingleton<DatabasePerformanceInterceptor>();
            services.AddDbContextFactory<AppDbContext>((provider, options) =>
                options
                    .UseSqlite($"Data Source={databasePath}")
                    .AddInterceptors(provider.GetRequiredService<DatabasePerformanceInterceptor>()));

            return services;
        }

        public static void InitializeDatabase(IServiceProvider services)
            => InitializeDatabaseAsync(services).GetAwaiter().GetResult();

        public static async Task InitializeDatabaseAsync(
            IServiceProvider services,
            CancellationToken ct = default)
        {
            var factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
            await using var db = await factory.CreateDbContextAsync(ct);
            var legacyDatabase = await TableExistsAsync(db, "Tasks", ct);
            var appliedMigrations = (await db.Database.GetAppliedMigrationsAsync(ct)).ToHashSet();

            // Una instalación anterior no tiene historial EF. Se completa una sola vez
            // con los upgrades heredados y se marca el baseline; las siguientes versiones
            // se aplican exclusivamente mediante migraciones.
            if (legacyDatabase && appliedMigrations.Count == 0)
                await BootstrapLegacyDatabaseAsync(db, ct);

            await db.Database.MigrateAsync(ct);
        }

        private static async Task BootstrapLegacyDatabaseAsync(AppDbContext db, CancellationToken ct)
        {
            await EnsureCourseAppraisalColumnsAsync(db, ct);
            await EnsureStudyTechniqueSessionColumnsAsync(db, ct);
            await EnsureExamSimulationTableAsync(db, ct);
            await EnsureReminderHistoryTableAsync(db, ct);
            await EnsureReminderSchedulesTableAsync(db, ct);

            var baseline = db.Database.GetMigrations().First();
            await db.Database.ExecuteSqlRawAsync(
                "CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (\"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY, \"ProductVersion\" TEXT NOT NULL);",
                ct);
            await db.Database.ExecuteSqlAsync(
                $"INSERT OR IGNORE INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ({baseline}, {"11.0.0"});",
                ct);
        }

        private static async Task EnsureReminderSchedulesTableAsync(AppDbContext db, CancellationToken ct)
        {
            if (!await TableExistsAsync(db, "ReminderSchedules", ct))
            {
                await db.Database.ExecuteSqlRawAsync(
                    """
                    CREATE TABLE IF NOT EXISTS "ReminderSchedules" (
                        "TaskId" TEXT NOT NULL,
                        "Kind" TEXT NOT NULL,
                        "Title" TEXT NOT NULL,
                        "Description" TEXT NULL,
                        "ScheduledForUtc" TEXT NOT NULL,
                        "DueDateUtc" TEXT NULL,
                        "Priority" INTEGER NOT NULL,
                        "Status" INTEGER NOT NULL,
                        "UpdatedAtUtc" TEXT NOT NULL,
                        "Attempts" INTEGER NOT NULL DEFAULT 0,
                        "NextAttemptUtc" TEXT NULL,
                        "LastError" TEXT NULL,
                        CONSTRAINT "PK_ReminderSchedules" PRIMARY KEY ("TaskId", "Kind")
                    );
                    CREATE INDEX IF NOT EXISTS "IX_ReminderSchedules_Status_ScheduledForUtc"
                        ON "ReminderSchedules" ("Status", "ScheduledForUtc");
                    """, ct);
            }

            var columns = await GetColumnsAsync(db, "ReminderSchedules", ct);
            await AddColumnIfMissingAsync(db, columns, "ReminderSchedules", "Attempts", ct);
            await AddColumnIfMissingAsync(db, columns, "ReminderSchedules", "NextAttemptUtc", ct);
            await AddColumnIfMissingAsync(db, columns, "ReminderSchedules", "LastError", ct);
        }

        private static async Task EnsureReminderHistoryTableAsync(AppDbContext db, CancellationToken ct)
        {
            if (await TableExistsAsync(db, "ReminderHistory", ct))
                return;

            await db.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "ReminderHistory" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_ReminderHistory" PRIMARY KEY,
                    "TaskId" TEXT NOT NULL,
                    "Kind" TEXT NOT NULL,
                    "Status" INTEGER NOT NULL,
                    "Title" TEXT NOT NULL,
                    "ScheduledForUtc" TEXT NULL,
                    "OccurredAtUtc" TEXT NOT NULL,
                    "Details" TEXT NULL
                );
                CREATE INDEX IF NOT EXISTS "IX_ReminderHistory_TaskId_OccurredAtUtc"
                    ON "ReminderHistory" ("TaskId", "OccurredAtUtc");
                CREATE INDEX IF NOT EXISTS "IX_ReminderHistory_OccurredAtUtc"
                    ON "ReminderHistory" ("OccurredAtUtc");
                """, ct);
        }

        private static async Task EnsureCourseAppraisalColumnsAsync(AppDbContext db, CancellationToken ct)
        {
            var columns = await GetColumnsAsync(db, "Courses", ct);
            if (columns.Count == 0)
                return;

            await AddColumnIfMissingAsync(db, columns, "Courses", "InterestLevel", ct);
            await AddColumnIfMissingAsync(db, columns, "Courses", "DifficultyLevel", ct);
            await AddColumnIfMissingAsync(db, columns, "Courses", "AppraisedAtUtc", ct);
        }

        private static async Task EnsureStudyTechniqueSessionColumnsAsync(AppDbContext db, CancellationToken ct)
        {
            var columns = await GetColumnsAsync(db, "StudyTechniqueSessions", ct);
            if (columns.Count == 0)
                return;

            await AddColumnIfMissingAsync(db, columns, "StudyTechniqueSessions", "TaskId", ct);
        }

        private static async Task EnsureExamSimulationTableAsync(AppDbContext db, CancellationToken ct)
        {
            if (await TableExistsAsync(db, "ExamSimulations", ct))
                return;

            // EnsureCreated no crea tablas nuevas en bases existentes.
            // Esta tabla mantiene el historial de simulacros por examen.
            await db.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "ExamSimulations" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_ExamSimulations" PRIMARY KEY,
                    "ExamTaskId" TEXT NOT NULL,
                    "Score" INTEGER NOT NULL,
                    "Errors" TEXT NULL,
                    "TakenAtUtc" TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS "IX_ExamSimulations_ExamTaskId_TakenAtUtc"
                    ON "ExamSimulations" ("ExamTaskId", "TakenAtUtc");
                """, ct);
        }

        private static async Task<bool> TableExistsAsync(
            AppDbContext db,
            string table,
            CancellationToken ct)
        {
            var connection = db.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;
            if (shouldClose)
                await connection.OpenAsync(ct);

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText =
                    "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name;";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "$name";
                parameter.Value = table;
                command.Parameters.Add(parameter);
                return Convert.ToInt64(await command.ExecuteScalarAsync(ct)) > 0;
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }

        private static async Task<HashSet<string>> GetColumnsAsync(
            AppDbContext db,
            string table,
            CancellationToken ct)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var connection = db.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;
            if (shouldClose)
                await connection.OpenAsync(ct);

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info('{table.Replace("'", "''")}');";
                await using var reader = await command.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                    result.Add(reader.GetString(1));
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }

            return result;
        }

        private static async Task AddColumnIfMissingAsync(
            AppDbContext db,
            HashSet<string> columns,
            string table,
            string name,
            CancellationToken ct)
        {
            if (columns.Contains(name))
                return;

            // SQLite no permite parametrizar nombres de columna; el switch evita SQL construido.
            var sql = (table, name) switch
            {
                ("Courses", "InterestLevel") =>
                    "ALTER TABLE \"Courses\" ADD COLUMN \"InterestLevel\" INTEGER NULL;",
                ("Courses", "DifficultyLevel") =>
                    "ALTER TABLE \"Courses\" ADD COLUMN \"DifficultyLevel\" INTEGER NULL;",
                ("Courses", "AppraisedAtUtc") =>
                    "ALTER TABLE \"Courses\" ADD COLUMN \"AppraisedAtUtc\" TEXT NULL;",
                ("StudyTechniqueSessions", "TaskId") =>
                    "ALTER TABLE \"StudyTechniqueSessions\" ADD COLUMN \"TaskId\" TEXT NULL;",
                ("ReminderSchedules", "Attempts") =>
                    "ALTER TABLE \"ReminderSchedules\" ADD COLUMN \"Attempts\" INTEGER NOT NULL DEFAULT 0;",
                ("ReminderSchedules", "NextAttemptUtc") =>
                    "ALTER TABLE \"ReminderSchedules\" ADD COLUMN \"NextAttemptUtc\" TEXT NULL;",
                ("ReminderSchedules", "LastError") =>
                    "ALTER TABLE \"ReminderSchedules\" ADD COLUMN \"LastError\" TEXT NULL;",
                _ => throw new ArgumentOutOfRangeException(nameof(name))
            };

            await db.Database.ExecuteSqlRawAsync(sql, ct);
        }
    }
}
