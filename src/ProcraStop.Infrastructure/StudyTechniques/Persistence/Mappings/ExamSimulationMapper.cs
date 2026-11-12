using ProcraStop.Domain.StudyTechniques.Entities;
using ProcraStop.Infrastructure.StudyTechniques.Persistence.Entities;

namespace ProcraStop.Infrastructure.StudyTechniques.Persistence.Mappings;

public static class ExamSimulationMapper
{
    public static ExamSimulationEntity ToEntity(ExamSimulation simulation) => new()
    {
        Id = simulation.Id,
        ExamTaskId = simulation.ExamTaskId,
        Score = simulation.Score,
        Errors = simulation.Errors,
        TakenAtUtc = simulation.TakenAtUtc
    };

    public static ExamSimulation ToDomain(ExamSimulationEntity entity) =>
        ExamSimulation.Rehydrate(
            entity.Id,
            entity.ExamTaskId,
            entity.Score,
            entity.Errors,
            entity.TakenAtUtc);
}
