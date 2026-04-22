

using Microsoft.EntityFrameworkCore;
using ProcraStop.Modules.Habits.Application.Abstractions;
using ProcraStop.Modules.Habits.Domain.Entities;
using ProcraStop.Modules.Habits.Infraestructure.Mappers;
using ProcraStop.Modules.Tasks.Infraestructure.Persistence;

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
    }
}
