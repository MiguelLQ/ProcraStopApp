using ProcraStop.Application.StudyTechniques.Abstractions;
using ProcraStop.Application.StudyTechniques.DTOs;
using ProcraStop.Domain.StudyTechniques.Entities;
using ProcraStop.Domain.Tasks.Enums;
using ProcraStop.Application.Tasks.Abstractions;

namespace ProcraStop.Application.StudyTechniques.Services;

// D2 EPA: registra simulacros y resume su evolucion.
public sealed class ExamSimulationService
{
    private readonly IExamSimulationRepository _repository;
    private readonly ITaskReadService _taskReadService;

    public ExamSimulationService(
        IExamSimulationRepository repository,
        ITaskReadService taskReadService)
    {
        _repository = repository;
        _taskReadService = taskReadService;
    }

    public async Task RecordAsync(
        Guid examTaskId,
        int score,
        string? errors,
        CancellationToken ct = default)
    {
        var exam = await _taskReadService.GetByIdAsync(examTaskId, ct);
        if (exam is null || exam.ActivityType != AcademicActivityType.Exam)
            throw new InvalidOperationException("El examen indicado no existe o no es un examen.");

        var simulation = ExamSimulation.Create(examTaskId, score, errors);
        await _repository.AddAsync(simulation, ct);
    }

    public async Task<ExamSimulationSummaryDto> GetSummaryAsync(
        Guid examTaskId,
        CancellationToken ct = default)
    {
        var simulations = await _repository.GetByExamTaskIdAsync(examTaskId, ct);
        return BuildSummary(examTaskId, simulations);
    }

    public static ExamSimulationSummaryDto BuildSummary(
        Guid examTaskId,
        IReadOnlyList<ExamSimulation> simulations)
    {
        if (simulations.Count == 0)
            return new ExamSimulationSummaryDto { ExamTaskId = examTaskId };

        var ordered = simulations.OrderBy(simulation => simulation.TakenAtUtc).ToArray();
        var last = ordered[^1];

        return new ExamSimulationSummaryDto
        {
            ExamTaskId = examTaskId,
            Attempts = ordered.Length,
            FirstScore = ordered[0].Score,
            LastScore = last.Score,
            BestScore = ordered.Max(simulation => simulation.Score),
            LastTakenAtUtc = last.TakenAtUtc,
            LastErrors = last.Errors
        };
    }
}
