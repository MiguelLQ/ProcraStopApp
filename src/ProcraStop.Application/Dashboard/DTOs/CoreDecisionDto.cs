

using ProcraStop.Core.Decisions.DTOs;

namespace ProcraStop.Application.Dashboard.DTOs
{
    public class CoreDecisionDto
    {
        public string Message { get; set; } = string.Empty;
        public DecisionType Type { get; set; }
        public string? Reason { get; set; }
        public string? SuggestedAction { get; set; }
    }
}
