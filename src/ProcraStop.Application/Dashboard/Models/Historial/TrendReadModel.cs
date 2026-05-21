namespace ProcraStop.Application.Dashboard.Models.Historial
{
    public class TrendReadModel
    {
        public TrendDirection StressTrend { get; set; }
        public TrendDirection PerformanceTrend { get; set; }
        public TrendDirection BehaviorTrend { get; set; }

        public string? GlobalTrend { get; set; } // Improving / Stable / Degrading
    }
}
