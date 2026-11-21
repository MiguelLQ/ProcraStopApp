
using Microsoft.EntityFrameworkCore;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Infrastructure.Dashboard.Persistence.Mappings;
using ProcraStop.Application.Dashboard.Models.Historial;
using ProcraStop.Application.Dashboard.Abstractions;
namespace ProcraStop.Infrastructure.Dashboard.Repositories
{
    public class SystemSnapshotRepository : ISystemSnapshotRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public SystemSnapshotRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<SystemSnapshot>> GetLastDays(int days, CancellationToken ct = default)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(ct);
            var from = DateTime.UtcNow.AddDays(-days);

            var entities = await context.SystemSnapshots
                .AsNoTracking()
                .Where(x => x.Timestamp >= from)
                .OrderBy(x => x.Timestamp)
                .ToListAsync(ct);

            return entities
                .Select(SystemSnapshotMapper.ToDomain)
                .ToList();
        }

        public async Task SaveAsync(SystemSnapshot snapshot, CancellationToken ct = default)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(ct);
            var entity = SystemSnapshotMapper.ToEntity(snapshot);

            context.SystemSnapshots.Add(entity);

            await context.SaveChangesAsync(ct);
        }
    }
}
