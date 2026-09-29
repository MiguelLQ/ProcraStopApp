
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
            // Durante desarrollo, AppDbContext es la unica fuente del esquema.
            // EnsureCreated crea todas las tablas del modelo en una base nueva.
            db.Database.EnsureCreated();
        }
    }
}
