

using Microsoft.EntityFrameworkCore;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Modules.Habits.Application.Abstractions;
using ProcraStop.Modules.Habits.Domain.Entities;
using ProcraStop.Modules.Habits.Infraestructure.Mappers;

namespace ProcraStop.Modules.Habits.Infraestructure.Repositories
{
    public class HabitLogRepository : IHabitLogRepository
    {
        private readonly AppDbContext _context;

        public HabitLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(HabitLog log)
        {
            var entity = HabitLogMapper.ToEntity(log);

            _context.HabitLogs.Add(entity);

            await _context.SaveChangesAsync();
        }

        public async Task<List<HabitLog>> GetAllAsync()
        {
            var entities = await _context.HabitLogs
                .AsNoTracking()
                .ToListAsync();

            return entities
                .Select(HabitLogMapper.ToDomain)
                .ToList();
        }
        public async Task<HabitLog?> GetByHabitAndDateAsync(Guid habitId, DateTime date)
        {
            var entity = await _context.HabitLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.HabitId == habitId &&
                    x.Date == date.Date);

            return entity == null ? null : HabitLogMapper.ToDomain(entity);
        }
        public async Task UpsertAsync(HabitLog log)
        {
            var existing = await _context.HabitLogs
                .FirstOrDefaultAsync(x =>
                    x.HabitId == log.HabitId &&
                    x.Date == log.Date);

            if (existing == null)
            {
                var entity = HabitLogMapper.ToEntity(log);
                _context.HabitLogs.Add(entity);
            }
            else
            {
                existing.Value = log.Value;
                existing.Notes = log.Notes;
            }

            await _context.SaveChangesAsync();
        }
    }
}
