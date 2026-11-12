using ProcraStop.Domain.StudyTechniques.Entities;

namespace ProcraStop.Application.StudyTechniques.Abstractions;

public interface IExamSimulationRepository
{
    Task AddAsync(ExamSimulation simulation, CancellationToken ct);
    Task<IReadOnlyList<ExamSimulation>> GetByExamTaskIdAsync(Guid examTaskId, CancellationToken ct);
}
