
using ProcraStop.Core.Decisions.DTOs;

namespace ProcraStop.Modules.Dashboard.Application.DTOs
{
    public class DashboardDecisionDto
    {
        public CoreDecisionDto CoreState { get; set; }

        public List<SignalDto> Signals { get; set; }

        public List<string> TaskInsights { get; set; }
        public List<string> HabitInsights { get; set; }
        public List<string> StressInsights { get; set; }
    }
}
