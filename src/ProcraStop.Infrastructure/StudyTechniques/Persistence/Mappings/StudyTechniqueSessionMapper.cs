using ProcraStop.Domain.StudyTechniques.Entities;
using ProcraStop.Infrastructure.StudyTechniques.Persistence.Entities;

namespace ProcraStop.Infrastructure.StudyTechniques.Persistence.Mappings;

public static class StudyTechniqueSessionMapper
{
    public static StudyTechniqueSessionEntity ToEntity(StudyTechniqueSession session)
    {
        return new StudyTechniqueSessionEntity
        {
            Id = session.Id,
            TaskId = session.TaskId,
            CourseName = session.CourseName,
            Topic = session.Topic,
            Technique = session.Technique,
            PlannedMinutes = session.PlannedMinutes,
            CompletedMinutes = session.CompletedMinutes,
            FocusBefore = session.FocusBefore,
            EnergyBefore = session.EnergyBefore,
            ConfidenceBefore = session.ConfidenceBefore,
            ConfusionAfter = session.ConfusionAfter,
            ProgressPercent = session.ProgressPercent,
            PomodoroCycles = session.PomodoroCycles,
            HelpStatus = session.HelpStatus,
            StartedAtUtc = session.StartedAtUtc,
            CompletedAtUtc = session.CompletedAtUtc,
            Notes = session.Notes
        };
    }

    public static StudyTechniqueSession ToDomain(StudyTechniqueSessionEntity entity)
    {
        return StudyTechniqueSession.Rehydrate(
            entity.Id,
            entity.TaskId,
            entity.CourseName,
            entity.Topic,
            entity.Technique,
            entity.PlannedMinutes,
            entity.CompletedMinutes,
            entity.FocusBefore,
            entity.EnergyBefore,
            entity.ConfidenceBefore,
            entity.ConfusionAfter,
            entity.ProgressPercent,
            entity.PomodoroCycles,
            entity.HelpStatus,
            entity.StartedAtUtc,
            entity.CompletedAtUtc,
            entity.Notes);
    }
}
