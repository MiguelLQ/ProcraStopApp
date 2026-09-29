using ProcraStop.Domain.StudyTechniques.Enums;

namespace ProcraStop.Domain.StudyTechniques.Entities;

public sealed class AcademicHelpRequest
{
    private AcademicHelpRequest() { }

    public Guid Id { get; private set; }
    public Guid? TaskId { get; private set; }
    public Guid? CourseId { get; private set; }
    public string Topic { get; private set; } = string.Empty;
    public string Question { get; private set; } = string.Empty;
    public HelpChannel Channel { get; private set; }
    public string? ContactName { get; private set; }
    public HelpRequestStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? RequestedAtUtc { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }

    public static AcademicHelpRequest Create(
        Guid? taskId,
        Guid? courseId,
        string topic,
        string question,
        HelpChannel channel,
        string? contactName)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("Escribe la duda que quieres consultar.", nameof(question));
        if (question.Trim().Length > 2000)
            throw new ArgumentException("La duda debe tener como maximo 2000 caracteres.", nameof(question));
        if (topic.Trim().Length > 200)
            throw new ArgumentException("El tema debe tener como maximo 200 caracteres.", nameof(topic));
        if (contactName?.Trim().Length > 150)
            throw new ArgumentException("El nombre debe tener como maximo 150 caracteres.", nameof(contactName));
        if (!Enum.IsDefined(channel))
            throw new ArgumentOutOfRangeException(nameof(channel));

        return new AcademicHelpRequest
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            CourseId = courseId,
            Topic = string.IsNullOrWhiteSpace(topic) ? "Tema libre" : topic.Trim(),
            Question = question.Trim(),
            Channel = channel,
            ContactName = string.IsNullOrWhiteSpace(contactName) ? null : contactName.Trim(),
            Status = HelpRequestStatus.Planned,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkRequested()
    {
        if (Status != HelpRequestStatus.Planned)
            throw new InvalidOperationException("La consulta ya fue registrada como realizada.");
        Status = HelpRequestStatus.Requested;
        RequestedAtUtc = DateTime.UtcNow;
    }

    public void MarkResolved()
    {
        if (Status != HelpRequestStatus.Requested)
            throw new InvalidOperationException("Primero registra que realizaste la consulta.");
        Status = HelpRequestStatus.Resolved;
        ResolvedAtUtc = DateTime.UtcNow;
    }

    public static AcademicHelpRequest Rehydrate(
        Guid id, Guid? taskId, Guid? courseId, string topic, string question,
        HelpChannel channel, string? contactName, HelpRequestStatus status,
        DateTime createdAtUtc, DateTime? requestedAtUtc, DateTime? resolvedAtUtc)
        => new()
        {
            Id = id,
            TaskId = taskId,
            CourseId = courseId,
            Topic = topic,
            Question = question,
            Channel = channel,
            ContactName = contactName,
            Status = status,
            CreatedAtUtc = createdAtUtc,
            RequestedAtUtc = requestedAtUtc,
            ResolvedAtUtc = resolvedAtUtc
        };
}
