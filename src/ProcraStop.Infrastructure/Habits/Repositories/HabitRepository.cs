using Microsoft.EntityFrameworkCore;
using ProcraStop.Application.Habits.Abstractions;
using ProcraStop.Domain.Habits.Entities;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Infrastructure.Habits.Persistence.Mappings;

namespace ProcraStop.Infrastructure.Habits.Repositories
{
    public class HabitRepository : IHabitRepository
    {
        private readonly AppDbContext _context;

        public HabitRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Habit habit)
        {
            var entity = HabitMapper.ToEntity(habit);

            _context.Habits.Add(entity);

            await _context.SaveChangesAsync();
        }

        public async Task<List<Habit>> GetAllAsync()
        {
            var entities = await _context.Habits
                .AsNoTracking()
                .ToListAsync();

            return entities
                .Select(HabitMapper.ToDomain)
                .ToList();
        }

        public async Task<IReadOnlyList<Habit>> GetActiveAsync(CancellationToken ct)
        {
            var entities = await _context.Habits
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync(ct);

            return entities.Select(HabitMapper.ToDomain).ToList();
        }

        public async Task<Habit?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var entity = await _context.Habits
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            return entity is null ? null : HabitMapper.ToDomain(entity); // editbycodex: consulta directa, no carga toda la tabla.
        }

        public async Task UpdateAsync(Habit habit, CancellationToken ct)
        {
            var existing = await _context.Habits.FirstOrDefaultAsync(x => x.Id == habit.Id, ct);
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
            await _context.SaveChangesAsync(ct);
        }
    }
}
