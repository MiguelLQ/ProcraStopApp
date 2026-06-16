
using ProcraStop.Core.Decisions.DTOs;

namespace ProcraStop.Core.Decisions.Signals
{
    public class UnifiedSignal
    {
        public string? Name { get; set; }
        public string? Category { get; set; }   // Stress / Performance / Habit / Task
        public double Score { get; set; }
        public DecisionType Type { get; set; }

        public string? Source { get; set; } // Tasks / Habits / Stress
    }
}
