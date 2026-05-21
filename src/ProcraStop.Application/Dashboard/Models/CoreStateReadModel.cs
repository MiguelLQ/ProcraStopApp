using ProcraStop.Core.Decisions.DTOs;

namespace ProcraStop.Application.Dashboard.Models
{
    public class CoreStateReadModel
    {
        public string Message { get; set; } = string.Empty;
        public DecisionType Type { get; set; }
        public string? Reason { get; set; }
        public string? SuggestedAction { get; set; }

        // opcional pero clave para futuro scoring UI
        public double Score { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}
