

using Microsoft.EntityFrameworkCore;
using ProcraStop.Application.Habits.Abstractions;
using ProcraStop.Domain.Habits.Entities;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Infrastructure.Habits.Persistence.Mappings;
using ProcraStop.Application.Dashboard.Interfaces;

namespace ProcraStop.Modules.Habits.Infraestructure.Repositories
{
    public class HabitLogRepository : IHabitLogRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly IDashboardCacheInvalidator _dashboardCache;

        public HabitLogRepository(
            IDbContextFactory<AppDbContext> dbContextFactory,
            IDashboardCacheInvalidator dashboardCache)
        {
            _dbContextFactory = dbContextFactory;
            _dashboardCache = dashboardCache;
        }

        public async Task AddAsync(HabitLog log)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = HabitLogMapper.ToEntity(log);

            context.HabitLogs.Add(entity);

            await context.SaveChangesAsync();
            _dashboardCache.Invalidate();
        }

        public async Task<List<HabitLog>> GetAllAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = await context.HabitLogs
                .AsNoTracking()
                .ToListAsync();

            return entities
                .Select(HabitLogMapper.ToDomain)
                .ToList();
        }
        public async Task<HabitLog?> GetByHabitAndDateAsync(Guid habitId, DateTime date)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.HabitLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.HabitId == habitId &&
                    x.Date == date.Date);

            return entity == null ? null : HabitLogMapper.ToDomain(entity);
        }
        public async Task UpsertAsync(HabitLog log)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var existing = await context.HabitLogs
                .FirstOrDefaultAsync(x =>
                    x.HabitId == log.HabitId &&
                    x.Date == log.Date);

            if (existing == null)
            {
                var entity = HabitLogMapper.ToEntity(log);
                context.HabitLogs.Add(entity);
            }
            else
            {
                existing.Value = log.Value;
                existing.Notes = log.Notes;
            }

            await context.SaveChangesAsync();
            _dashboardCache.Invalidate();
        }

        public async Task<IReadOnlyList<HabitLog>> GetRangeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(ct);
            var start = startDate.Date;
            var endExclusive = endDate.Date.AddDays(1);
            var entities = await context.HabitLogs
                .AsNoTracking()
                .Where(x => x.Date >= start && x.Date < endExclusive)
                .ToListAsync(ct);

            return entities.Select(HabitLogMapper.ToDomain).ToList(); // editbycodex: solo historial necesario para rachas.
        }

        public async Task<IReadOnlyList<HabitLog>> GetRangeForHabitAsync(
            Guid habitId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(ct);
            var start = startDate.Date;
            var endExclusive = endDate.Date.AddDays(1);
            var entities = await context.HabitLogs
                .AsNoTracking()
                .Where(x =>
                    x.HabitId == habitId &&
                    x.Date >= start &&
                    x.Date < endExclusive)
                .ToListAsync(ct);

            return entities.Select(HabitLogMapper.ToDomain).ToList();
        }
    }
}
