
using Microsoft.EntityFrameworkCore;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Infrastructure.Stress.Persistence.Mappings;
using ProcraStop.Modules.Stress.Application.Abstractions;
using ProcraStop.Modules.Stress.Domain.Entities;
using ProcraStop.Modules.Stress.Domain.Enums;
using ProcraStop.Modules.Stress.Infrastructure.Persistences.Entities;
using ProcraStop.Modules.Stress.Infrastructure.Persistences.Mappers;
using ProcraStop.Modules.Tasks.Infraestructure.Persistence;

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
        public async Task<bool> ExistsTodayAsync(StressSource source)
        {
            var today = DateTime.Now.Date;

            return await _context.StressRecords
                .AnyAsync(x =>
                    x.Source == source &&
                    x.Timestamp.Date == today);
        }
        public async Task UpdateTodayAsync(double level, StressSource source)
        {
            var today = DateTime.Now.Date;

            var entity = await _context.StressRecords
                .Where(x =>
                    x.Source == source && // o directo enum si haces conversión
                    x.Timestamp.Date == today)
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync();

            if (entity is null)
            {
                // fallback → crear si no existe
                var newEntity = new StressRecordEntity
                {
                    Id = Guid.NewGuid(),
                    Level = level,
                    Source = source,
                    Timestamp = DateTime.UtcNow
                };

                _context.StressRecords.Add(newEntity);
            }
            else
            {
                // actualizar el existente
                entity.Level = level;
                entity.Timestamp = DateTime.UtcNow; // importante: refresh
            }

            await _context.SaveChangesAsync();
        }
        public async Task<bool> ExistsInLastHoursAsync(StressSource source, int hours)
        {
            var cutoff = DateTime.UtcNow.AddHours(-hours);

            return await _context.StressRecords
                .AnyAsync(x =>
                    x.Source == source &&
                    x.Timestamp >= cutoff);
        }
    }
}
