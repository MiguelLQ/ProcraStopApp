using ProcraStop.Domain.StudyTechniques.Entities;

namespace ProcraStop.Modules.StudyTechniques.Presentation.Models;

public sealed class StudySessionHistoryItem
{
    public StudySessionHistoryItem(StudyTechniqueSession session)
    {
        CourseName = session.CourseName;
        Topic = session.Topic;
        TechniqueName = session.Technique.ToString();
        DateText = session.StartedAtUtc.ToLocalTime().ToString("dd MMM, HH:mm");
        DetailText = $"{session.CompletedMinutes} min · avance {session.ProgressPercent}% · confusión {session.ConfusionAfter}/5";
        Notes = string.IsNullOrWhiteSpace(session.Notes) ? "Sin nota" : session.Notes;
    }

    public string CourseName { get; }
    public string Topic { get; }
    public string TechniqueName { get; }
    public string DateText { get; }
    public string DetailText { get; }
    public string Notes { get; }
}
