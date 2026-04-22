using Microsoft.EntityFrameworkCore;
using ProcraStop.Modules.Habits.Application.Abstractions;
using ProcraStop.Modules.Habits.Domain.Entities;
using ProcraStop.Modules.Habits.Infraestructure.Mappers;
using ProcraStop.Modules.Tasks.Infraestructure.Persistence;

namespace ProcraStop.Modules.Habits.Infraestructure.Repositories
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
