

using ProcraStop.Core.Decisions.DTOs;

namespace ProcraStop.Application.Dashboard.Models
{
    public class SignalReadModel
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Score { get; set; }
        public DecisionType Type { get; set; }
    }
}
