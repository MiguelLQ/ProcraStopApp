using Microsoft.EntityFrameworkCore;
using ProcraStop.Application.Academic.Abstractions;
using ProcraStop.Domain.Academic.Entities;
using ProcraStop.Infrastructure.Academic.Persistence.Mappings;
using ProcraStop.Infrastructure.DBContext;

namespace ProcraStop.Infrastructure.Academic.Repositories;

public sealed class ClassScheduleRepository : IClassScheduleRepository
{
    private readonly AppDbContext _dbContext;

    public ClassScheduleRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ClassSchedule>> GetAllActiveAsync(CancellationToken ct)
    {
        var entities = await _dbContext.ClassSchedules
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync(ct);

        return entities.Select(x => x.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<ClassSchedule>> GetByDayAsync(DayOfWeek dayOfWeek, CancellationToken ct)
    {
        var day = (int)dayOfWeek;
        var entities = await _dbContext.ClassSchedules
            .AsNoTracking()
            .Where(x => x.IsActive && x.DayOfWeek == day)
            .OrderBy(x => x.StartTicks)
            .ToListAsync(ct);

        return entities.Select(x => x.ToDomain()).ToList();
    }

    public async Task<ClassSchedule?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await _dbContext.ClassSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity?.ToDomain();
    }

    public async Task AddAsync(ClassSchedule schedule, CancellationToken ct)
    {
        await _dbContext.ClassSchedules.AddAsync(schedule.ToEntity(), ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ClassSchedule schedule, CancellationToken ct)
    {
        _dbContext.ClassSchedules.Update(schedule.ToEntity());
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _dbContext.ClassSchedules.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
            return;

        _dbContext.ClassSchedules.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
    }
}
