using ProcraStop.Application.StudyTechniques.Abstractions;
using ProcraStop.Domain.StudyTechniques.Entities;

namespace ProcraStop.Application.StudyTechniques.Commands.RegisterStudySession;

public sealed class RegisterStudySessionHandler
{
    private readonly IStudyTechniqueSessionRepository _repository;

    public RegisterStudySessionHandler(IStudyTechniqueSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(RegisterStudySessionCommand command, CancellationToken ct)
    {
        var session = StudyTechniqueSession.Create(
            command.CourseName,
            command.Topic,
            command.Technique,
            command.PlannedMinutes,
            command.CompletedMinutes,
            command.FocusBefore,
            command.EnergyBefore,
            command.ConfidenceBefore,
            command.ConfusionAfter,
            command.ProgressPercent,
            command.PomodoroCycles,
            command.HelpStatus,
            command.Notes);

        await _repository.AddAsync(session, ct);
    }
}
