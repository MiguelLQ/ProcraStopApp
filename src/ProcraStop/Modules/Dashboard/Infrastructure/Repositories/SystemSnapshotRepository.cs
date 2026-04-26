
using ProcraStop.Modules.Dashboard.Application.Abstractions;
using ProcraStop.Modules.Dashboard.Application.Models.Historial;
using ProcraStop.Modules.Tasks.Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ProcraStop.Modules.Dashboard.Infrastructure.Mappers;
namespace ProcraStop.Modules.Dashboard.Infrastructure.Repositories
{
    public class SystemSnapshotRepository : ISystemSnapshotRepository
    {
        private readonly AppDbContext _context;

        public SystemSnapshotRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SystemSnapshot>> GetLastDays(int days)
        {
            var from = DateTime.UtcNow.AddDays(-days);

            var entities = await _context.SystemSnapshots
                .Where(x => x.Timestamp >= from)
                .OrderBy(x => x.Timestamp)
                .ToListAsync();

            return entities
                .Select(SystemSnapshotMapper.ToDomain)
                .ToList();
        }

        public async Task SaveAsync(SystemSnapshot snapshot)
        {
            var entity = SystemSnapshotMapper.ToEntity(snapshot);

            _context.SystemSnapshots.Add(entity);

            await _context.SaveChangesAsync();
        }
    }
}
