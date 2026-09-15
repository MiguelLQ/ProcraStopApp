using ProcraStop.Application.Dashboard.Models.Historial;

namespace ProcraStop.Application.Dashboard.Services;

internal static class DashboardSnapshotPolicy
{
    public static bool ShouldRecord(SystemSnapshot? last, SystemSnapshot current)
    {
        if (last is null)
            return true;

        var minutes = (current.Timestamp - last.Timestamp).TotalMinutes;
        if (minutes >= 30)
            return true;

        var delta =
            Math.Abs(current.Stress - last.Stress) +
            Math.Abs(current.Performance - last.Performance) +
            Math.Abs(current.Behavior - last.Behavior);

        return delta >= 0.25;
    }
}
