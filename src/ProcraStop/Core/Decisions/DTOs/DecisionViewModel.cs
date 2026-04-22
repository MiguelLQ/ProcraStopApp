
namespace ProcraStop.Core.Decisions.DTOs
{
    public class DecisionViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public string? Reason { get; set; }

        public string? SuggestedAction { get; set; }

        public string Severity { get; set; } = "Info";
        // Info | Warning | Success | Critical

        public List<string> Insights { get; set; } = new();

        public List<string> SupportingInsights { get; set; } = new();

        public string? Icon { get; set; }

        public string? Color { get; set; }
    }
}
