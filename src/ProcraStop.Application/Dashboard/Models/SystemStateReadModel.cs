

namespace ProcraStop.Application.Dashboard.Models
{
    public class SystemStateReadModel
    {
        public double OverallScore { get; set; }
        public double StressPressure { get; set; }
        public double PerformanceHealth { get; set; }
        public double BehavioralStability { get; set; }
        public string? Label {  get; set; }
    }
}
