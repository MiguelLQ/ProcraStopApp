
using ProcraStop.Modules.Dashboard.Application.Models.Historial;

namespace ProcraStop.Modules.Dashboard.Application.Models
{
    public class DashboardReadModel
    {
        public CoreStateReadModel? CoreState { get; set; }
        public List<SignalReadModel>? Signals { get; set; }
        public SystemStateReadModel? SystemState { get; set; }
        public List<CategoryBalanceReadModel>? CategoryBalance { get; set; }
        public TrendReadModel? Trend { get; set; }
        public List<string>? TaskInsights { get; set; }
        public List<string>? HabitInsights { get; set; }
        public List<string>? StressInsights { get; set; }
    }
}
