using Microsoft.EntityFrameworkCore;
using ProcraStop.Application.Habits.Abstractions;
using ProcraStop.Domain.Habits.Entities;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Infrastructure.Habits.Persistence.Mappings;
using ProcraStop.Application.Dashboard.Interfaces;

namespace ProcraStop.Infrastructure.Habits.Repositories
{
    public class HabitRepository : IHabitRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly IDashboardCacheInvalidator _dashboardCache;

        public HabitRepository(
            IDbContextFactory<AppDbContext> dbContextFactory,
            IDashboardCacheInvalidator dashboardCache)
        {
            _dbContextFactory = dbContextFactory;
            _dashboardCache = dashboardCache;
        }

        public async Task AddAsync(Habit habit)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = HabitMapper.ToEntity(habit);

            context.Habits.Add(entity);

            await context.SaveChangesAsync();
            _dashboardCache.Invalidate();
        }

        public async Task<List<Habit>> GetAllAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = await context.Habits
                .AsNoTracking()
                .ToListAsync();

            return entities
                .Select(HabitMapper.ToDomain)
                .ToList();
        }

        public async Task<IReadOnlyList<Habit>> GetActiveAsync(CancellationToken ct)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(ct);
            var entities = await context.Habits
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync(ct);

            return entities.Select(HabitMapper.ToDomain).ToList();
        }

        public async Task<Habit?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(ct);
            var entity = await context.Habits
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            return entity is null ? null : HabitMapper.ToDomain(entity); // editbycodex: consulta directa, no carga toda la tabla.
        }

        public async Task UpdateAsync(Habit habit, CancellationToken ct)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(ct);
            var existing = await context.Habits.FirstOrDefaultAsync(x => x.Id == habit.Id, ct);
            if (existing is null)
                throw new InvalidOperationException("Hábito no encontrado.");

            var updated = HabitMapper.ToEntity(habit);
            existing.Name = updated.Name;
            existing.Unit = updated.Unit;
            existing.TargetValue = updated.TargetValue;
            existing.DaysOfWeek = updated.DaysOfWeek;
            existing.RequiredPercentage = updated.RequiredPercentage;
            existing.Weight = updated.Weight;
            existing.TrackingType = updated.TrackingType;
            existing.IsActive = updated.IsActive;
            await context.SaveChangesAsync(ct);
            _dashboardCache.Invalidate();
        }
    }
}
