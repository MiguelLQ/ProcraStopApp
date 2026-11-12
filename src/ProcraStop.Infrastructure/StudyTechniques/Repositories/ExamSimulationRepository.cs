using Microsoft.EntityFrameworkCore;
using ProcraStop.Application.StudyTechniques.Abstractions;
using ProcraStop.Domain.StudyTechniques.Entities;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Infrastructure.StudyTechniques.Persistence.Mappings;

namespace ProcraStop.Infrastructure.StudyTechniques.Repositories;

public sealed class ExamSimulationRepository : IExamSimulationRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public ExamSimulationRepository(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task AddAsync(ExamSimulation simulation, CancellationToken ct)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(ct);
        context.ExamSimulations.Add(ExamSimulationMapper.ToEntity(simulation));
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ExamSimulation>> GetByExamTaskIdAsync(
        Guid examTaskId,
        CancellationToken ct)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(ct);
        var entities = await context.ExamSimulations
            .AsNoTracking()
            .Where(x => x.ExamTaskId == examTaskId)
            .OrderByDescending(x => x.TakenAtUtc)
            .ToListAsync(ct);

        return entities.Select(ExamSimulationMapper.ToDomain).ToArray();
    }
}
