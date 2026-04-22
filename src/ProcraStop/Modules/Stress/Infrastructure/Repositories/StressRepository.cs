
using ProcraStop.Modules.Stress.Application.Abstractions;
using ProcraStop.Modules.Stress.Domain.Entities;
using ProcraStop.Modules.Stress.Infrastructure.Persistences.Mappers;
using ProcraStop.Modules.Tasks.Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ProcraStop.Modules.Stress.Infrastructure.Repositories
{
    public class StressRepository : IStressRepository
    {
        private readonly AppDbContext _context;

        public StressRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(StressRecord record)
        {
            var entity = StressMapper.ToEntity(record);

            _context.StressRecords.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<StressRecord>> GetRecentAsync(int days)
        {
            var cutoff = DateTime.UtcNow.AddDays(-days);

            var entities = await _context.StressRecords
                .Where(x => x.Timestamp >= cutoff)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();

            return entities
                .Select(StressMapper.ToDomain)
                .ToList();
        }

        public async Task<StressRecord?> GetLatestAsync()
        {
            var entity = await _context.StressRecords
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync();

            if (entity is null)
                return null;

            return StressMapper.ToDomain(entity);
        }
    }
}
