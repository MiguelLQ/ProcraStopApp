

using ProcraStop.Core.Decisions.DTOs;

namespace ProcraStop.Core.Decisions.Memory
{
    public class DecisionMemory
    {
        public string? LastRuleName { get; set; }
        public DecisionType? LastDecisionType { get; set; }
        public DateTime? LastDecisionTime { get; set; }

        public Dictionary<string, DateTime> RuleCooldowns { get; set; } = new();

        public int BurnoutAlertsLast24h { get; set; }
    }
}
