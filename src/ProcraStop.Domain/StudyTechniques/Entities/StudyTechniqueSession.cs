using ProcraStop.Domain.StudyTechniques.Enums;

namespace ProcraStop.Domain.StudyTechniques.Entities;

public sealed class StudyTechniqueSession
{
    private StudyTechniqueSession() { }

    private StudyTechniqueSession(
        Guid id,
        Guid? taskId,
        string courseName,
        string topic,
        StudyTechniqueType technique,
        int plannedMinutes,
        int completedMinutes,
        int focusBefore,
        int energyBefore,
        int confidenceBefore,
        int confusionAfter,
        int progressPercent,
        int pomodoroCycles,
        AcademicHelpStatus helpStatus,
        DateTime startedAtUtc,
        DateTime? completedAtUtc,
        string? notes)
    {
        Id = id;
        TaskId = taskId;
        CourseName = Normalize(courseName, "General");
        Topic = Normalize(topic, "Tema libre");
        Technique = technique;
        PlannedMinutes = Math.Max(1, plannedMinutes);
        CompletedMinutes = Math.Max(0, completedMinutes);
        FocusBefore = ClampScale(focusBefore);
        EnergyBefore = ClampScale(energyBefore);
        ConfidenceBefore = ClampScale(confidenceBefore);
        ConfusionAfter = ClampScale(confusionAfter);
        ProgressPercent = Math.Clamp(progressPercent, 0, 100);
        PomodoroCycles = Math.Max(0, pomodoroCycles);
        HelpStatus = helpStatus;
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public Guid Id { get; private set; }
    // D2 EPA: examen concreto al que pertenece la sesion (opcional).
    public Guid? TaskId { get; private set; }
    public string CourseName { get; private set; } = string.Empty;
    public string Topic { get; private set; } = string.Empty;
    public StudyTechniqueType Technique { get; private set; }
    public int PlannedMinutes { get; private set; }
    public int CompletedMinutes { get; private set; }
    public int FocusBefore { get; private set; }
    public int EnergyBefore { get; private set; }
    public int ConfidenceBefore { get; private set; }
    public int ConfusionAfter { get; private set; }
    public int ProgressPercent { get; private set; }
    public int PomodoroCycles { get; private set; }
    public AcademicHelpStatus HelpStatus { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public string? Notes { get; private set; }

    public static StudyTechniqueSession Create(
        string courseName,
        string topic,
        StudyTechniqueType technique,
        int plannedMinutes,
        int completedMinutes,
        int focusBefore,
        int energyBefore,
        int confidenceBefore,
        int confusionAfter,
        int progressPercent,
        int pomodoroCycles,
        AcademicHelpStatus helpStatus,
        string? notes,
        Guid? taskId = null)
    {
        return new StudyTechniqueSession(
            Guid.NewGuid(),
            taskId,
            courseName,
            topic,
            technique,
            plannedMinutes,
            completedMinutes,
            focusBefore,
            energyBefore,
            confidenceBefore,
            confusionAfter,
            progressPercent,
            pomodoroCycles,
            helpStatus,
            DateTime.UtcNow,
            DateTime.UtcNow,
            notes);
    }

    public static StudyTechniqueSession Rehydrate(
        Guid id,
        Guid? taskId,
        string courseName,
        string topic,
        StudyTechniqueType technique,
        int plannedMinutes,
        int completedMinutes,
        int focusBefore,
        int energyBefore,
        int confidenceBefore,
        int confusionAfter,
        int progressPercent,
        int pomodoroCycles,
        AcademicHelpStatus helpStatus,
        DateTime startedAtUtc,
        DateTime? completedAtUtc,
        string? notes)
    {
        return new StudyTechniqueSession(
            id,
            taskId,
            courseName,
            topic,
            technique,
            plannedMinutes,
            completedMinutes,
            focusBefore,
            energyBefore,
            confidenceBefore,
            confusionAfter,
            progressPercent,
            pomodoroCycles,
            helpStatus,
            startedAtUtc,
            completedAtUtc,
            notes);
    }

    private static int ClampScale(int value) => Math.Clamp(value, 1, 5);

    private static string Normalize(string value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
