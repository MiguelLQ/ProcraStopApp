using Microsoft.EntityFrameworkCore;
using ProcraStop.Application.Academic.Abstractions;
using ProcraStop.Domain.Academic.Entities;
using ProcraStop.Infrastructure.Academic.Persistence.Mappings;
using ProcraStop.Infrastructure.DBContext;

namespace ProcraStop.Infrastructure.Academic.Repositories;

public sealed class ClassScheduleRepository : IClassScheduleRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public ClassScheduleRepository(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<ClassSchedule>> GetAllActiveAsync(CancellationToken ct)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
        var entities = await dbContext.ClassSchedules
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync(ct);

        return entities.Select(x => x.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<ClassSchedule>> GetByDayAsync(DayOfWeek dayOfWeek, CancellationToken ct)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
        var day = (int)dayOfWeek;
        var entities = await dbContext.ClassSchedules
            .AsNoTracking()
            .Where(x => x.IsActive && x.DayOfWeek == day)
            .OrderBy(x => x.StartTicks)
            .ToListAsync(ct);

        return entities.Select(x => x.ToDomain()).ToList();
    }

    public async Task<ClassSchedule?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
        var entity = await dbContext.ClassSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity?.ToDomain();
    }

    public async Task AddAsync(ClassSchedule schedule, CancellationToken ct)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
        await dbContext.ClassSchedules.AddAsync(schedule.ToEntity(), ct);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ClassSchedule schedule, CancellationToken ct)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
        dbContext.ClassSchedules.Update(schedule.ToEntity());
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
        var entity = await dbContext.ClassSchedules.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
            return;

        dbContext.ClassSchedules.Remove(entity);
        await dbContext.SaveChangesAsync(ct);
    }
}
