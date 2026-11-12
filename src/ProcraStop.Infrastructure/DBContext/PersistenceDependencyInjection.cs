
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

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
            // Durante desarrollo, AppDbContext es la unica fuente del esquema.
            // EnsureCreated crea todas las tablas del modelo en una base nueva.
            db.Database.EnsureCreated();
            // EnsureCreated no agrega columnas a una base existente; este upgrade
            // mantiene compatibles las instalaciones previas sin migraciones EF.
            EnsureCourseAppraisalColumns(db);
            EnsureStudyTechniqueSessionColumns(db);
            EnsureExamSimulationTable(db);
        }

        private static void EnsureCourseAppraisalColumns(AppDbContext db)
        {
            var columns = GetColumns(db, "Courses");
            if (columns.Count == 0)
                return;

            AddColumnIfMissing(db, columns, "Courses", "InterestLevel");
            AddColumnIfMissing(db, columns, "Courses", "DifficultyLevel");
            AddColumnIfMissing(db, columns, "Courses", "AppraisedAtUtc");
        }

        private static void EnsureStudyTechniqueSessionColumns(AppDbContext db)
        {
            var columns = GetColumns(db, "StudyTechniqueSessions");
            if (columns.Count == 0)
                return;

            AddColumnIfMissing(db, columns, "StudyTechniqueSessions", "TaskId");
        }

        private static void EnsureExamSimulationTable(AppDbContext db)
        {
            if (TableExists(db, "ExamSimulations"))
                return;

            // EnsureCreated no crea tablas nuevas en bases existentes.
            // Esta tabla mantiene el historial de simulacros por examen.
            db.Database.ExecuteSqlRaw(
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
                """);
        }

        private static bool TableExists(AppDbContext db, string table)
        {
            var connection = db.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;
            if (shouldClose)
                connection.Open();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText =
                    "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name;";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "$name";
                parameter.Value = table;
                command.Parameters.Add(parameter);
                return Convert.ToInt64(command.ExecuteScalar()) > 0;
            }
            finally
            {
                if (shouldClose)
                    connection.Close();
            }
        }

        private static HashSet<string> GetColumns(AppDbContext db, string table)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var connection = db.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;
            if (shouldClose)
                connection.Open();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info('{table.Replace("'", "''")}');";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    result.Add(reader.GetString(1));
            }
            finally
            {
                if (shouldClose)
                    connection.Close();
            }

            return result;
        }

        private static void AddColumnIfMissing(
            AppDbContext db,
            HashSet<string> columns,
            string table,
            string name)
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
                _ => throw new ArgumentOutOfRangeException(nameof(name))
            };

            db.Database.ExecuteSqlRaw(sql);
        }
    }
}
