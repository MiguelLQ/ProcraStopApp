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
    }
}
