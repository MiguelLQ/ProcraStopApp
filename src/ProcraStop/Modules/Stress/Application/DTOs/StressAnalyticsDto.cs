

namespace ProcraStop.Modules.Stress.Application.DTOs
{
    public class StressAnalyticsDto
    {
        public double Average { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }

        public StressTrend Trend { get; set; }

        public int HighStressDays { get; set; }

        public double RecoveryScore { get; set; }
    }
}
