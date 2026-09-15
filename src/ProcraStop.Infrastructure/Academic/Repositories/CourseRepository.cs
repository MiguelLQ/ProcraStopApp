using Microsoft.EntityFrameworkCore;
using ProcraStop.Application.Academic.Abstractions;
using ProcraStop.Domain.Academic.Entities;
using ProcraStop.Infrastructure.Academic.Persistence.Mappings;
using ProcraStop.Infrastructure.DBContext;

namespace ProcraStop.Infrastructure.Academic.Repositories
{
    public sealed class CourseRepository : ICourseRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public CourseRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken ct)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
            var entities = await dbContext.Courses
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync(ct);

            return entities.Select(c => c.ToDomain()).ToList();
        }

        public async Task<Course?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
            var entity = await dbContext.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            return entity?.ToDomain();
        }

        public async Task AddAsync(Course course, CancellationToken ct)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
            var entity = course.ToEntity();

            await dbContext.Courses.AddAsync(entity, ct);
            await dbContext.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Course course, CancellationToken ct)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
            var entity = course.ToEntity();

            dbContext.Courses.Update(entity);
            await dbContext.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
            var entity = await dbContext.Courses
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (entity is null)
                return;

            dbContext.Courses.Remove(entity);
            await dbContext.SaveChangesAsync(ct);
        }
    }
}
