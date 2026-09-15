
using Microsoft.EntityFrameworkCore;
using ProcraStop.Application.Stress.Abstractions;
using ProcraStop.Domain.Stress.Entities;
using ProcraStop.Domain.Stress.Enums;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Infrastructure.Stress.Persistence.Entities;
using ProcraStop.Infrastructure.Stress.Persistence.Mappings;
using ProcraStop.Application.Dashboard.Interfaces;
namespace ProcraStop.Infrastructure.Stress.Repositories
{
    public class StressRepository : IStressRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly IDashboardCacheInvalidator _dashboardCache;

        public StressRepository(
            IDbContextFactory<AppDbContext> dbContextFactory,
            IDashboardCacheInvalidator dashboardCache)
        {
            _dbContextFactory = dbContextFactory;
            _dashboardCache = dashboardCache;
        }

        public async Task AddAsync(StressRecord record)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = StressMapper.ToEntity(record);

            context.StressRecords.Add(entity);
            await context.SaveChangesAsync();
            _dashboardCache.Invalidate();
        }

        public async Task<List<StressRecord>> GetRecentAsync(int days)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var cutoff = DateTime.UtcNow.AddDays(-days);

            var entities = await context.StressRecords
                .AsNoTracking()
                .Where(x => x.Timestamp >= cutoff)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();

            return entities
                .Select(StressMapper.ToDomain)
                .ToList();
        }

        public async Task<StressRecord?> GetLatestAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.StressRecords
                .AsNoTracking()
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync();

            if (entity is null)
                return null;

            return StressMapper.ToDomain(entity);
        }
        public async Task<bool> ExistsTodayAsync(StressSource source)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var today = DateTime.Now.Date;

            return await context.StressRecords
                .AnyAsync(x =>
                    x.Source == source &&
                    x.Timestamp.Date == today);
        }
        public async Task UpdateTodayAsync(double level, StressSource source)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var today = DateTime.Now.Date;

            var entity = await context.StressRecords
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

                context.StressRecords.Add(newEntity);
            }
            else
            {
                // actualizar el existente
                entity.Level = level;
                entity.Timestamp = DateTime.UtcNow; // importante: refresh
            }

            await context.SaveChangesAsync();
            _dashboardCache.Invalidate();
        }
        public async Task<bool> ExistsInLastHoursAsync(StressSource source, int hours)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var cutoff = DateTime.UtcNow.AddHours(-hours);

            return await context.StressRecords
                .AnyAsync(x =>
                    x.Source == source &&
                    x.Timestamp >= cutoff);
        }
    }
}
