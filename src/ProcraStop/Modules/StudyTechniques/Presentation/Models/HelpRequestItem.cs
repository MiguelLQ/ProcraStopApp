using ProcraStop.Domain.StudyTechniques.Entities;
using ProcraStop.Domain.StudyTechniques.Enums;

namespace ProcraStop.Modules.StudyTechniques.Presentation.Models;

public sealed class HelpRequestItem
{
    public Guid Id { get; init; }
    public string Topic { get; init; } = string.Empty;
    public string Question { get; init; } = string.Empty;
    public string ChannelText { get; init; } = string.Empty;
    public string StatusText { get; init; } = string.Empty;
    public string CreatedText { get; init; } = string.Empty;
    public bool CanMarkRequested { get; init; }
    public bool CanResolve { get; init; }

    public static HelpRequestItem FromDomain(AcademicHelpRequest request) => new()
    {
        Id = request.Id,
        Topic = request.Topic,
        Question = request.Question,
        ChannelText = HelpChannelOption.All.First(x => x.Value == request.Channel).Name +
            (request.ContactName is null ? "" : $" · {request.ContactName}"),
        StatusText = request.Status switch
        {
            HelpRequestStatus.Planned => "Por consultar",
            HelpRequestStatus.Requested => "Consultado, pendiente de resolver",
            HelpRequestStatus.Resolved => "Resuelto",
            _ => "Por consultar"
        },
        CreatedText = request.CreatedAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
        CanMarkRequested = request.Status == HelpRequestStatus.Planned,
        CanResolve = request.Status == HelpRequestStatus.Requested
    };
}
