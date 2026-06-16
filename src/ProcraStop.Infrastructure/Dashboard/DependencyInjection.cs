

using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Application.Dashboard.Abstractions;
using ProcraStop.Infrastructure.Dashboard.Repositories;

namespace ProcraStop.Infrastructure.Dashboard
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDashboardInfrastructure(
        this IServiceCollection services)
        {
            // SNAPSHOT
            services.AddScoped<ISystemSnapshotRepository, SystemSnapshotRepository>();
            return services;
        }
    }
}
