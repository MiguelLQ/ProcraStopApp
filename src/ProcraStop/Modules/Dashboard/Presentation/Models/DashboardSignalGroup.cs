using ProcraStop.Application.Dashboard.Models;

namespace ProcraStop.Modules.Dashboard.Presentation.Models;

public sealed class DashboardSignalGroup
{
    public string Category { get; init; } = string.Empty;
    public string AccentColor { get; init; } = "#4075FF";
    public IReadOnlyList<SignalReadModel> Signals { get; init; } = Array.Empty<SignalReadModel>();
}
