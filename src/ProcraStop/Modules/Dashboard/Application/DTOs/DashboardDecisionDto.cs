
using ProcraStop.Core.Decisions.DTOs;

namespace ProcraStop.Modules.Dashboard.Application.DTOs
{
    public class DashboardDecisionDto
    {
        public string Message { get; set; } = string.Empty;

        // ✔ ahora usa el tipo del CORE
        public DecisionType Type { get; set; }

        public List<string>? SecondaryInsights { get; set; } = new();

        public string? SuggestedAction { get; set; }

        public string? Reason { get; set; }

        // insights combinados (tasks + habits)
        public List<string>? Insights { get; set; } = new();
        // streesss
        public double StressLevel { get; set; }
        public string StressLabel { get; set; } = "";

    }
}
