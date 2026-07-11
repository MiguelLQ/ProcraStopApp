using Microsoft.EntityFrameworkCore;
using ProcraStop.Application.Academic.Abstractions;
using ProcraStop.Domain.Academic.Entities;
using ProcraStop.Infrastructure.Academic.Persistence.Mappings;
using ProcraStop.Infrastructure.DBContext;

namespace ProcraStop.Infrastructure.Academic.Repositories
{
    public sealed class CourseRepository : ICourseRepository
    {
        private readonly AppDbContext _dbContext;

        public CourseRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken ct)
        {
            var entities = await _dbContext.Courses
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync(ct);

            return entities.Select(c => c.ToDomain()).ToList();
        }

        public async Task<Course?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var entity = await _dbContext.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            return entity?.ToDomain();
        }

        public async Task AddAsync(Course course, CancellationToken ct)
        {
            var entity = course.ToEntity();

            await _dbContext.Courses.AddAsync(entity, ct);
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Course course, CancellationToken ct)
        {
            var entity = course.ToEntity();

            _dbContext.Courses.Update(entity);
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var entity = await _dbContext.Courses
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (entity is null)
                return;

            _dbContext.Courses.Remove(entity);
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
