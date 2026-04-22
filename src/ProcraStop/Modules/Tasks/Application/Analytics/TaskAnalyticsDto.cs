

namespace ProcraStop.Modules.Tasks.Application.Analytics
{
    public class TaskAnalyticsDto
    {
        public int Total { get; set; }
        public int Completed { get; set; }
        public int Pending { get; set; }
        public int Overdue { get; set; }

        public int HighPriority { get; set; }
        public int MediumPriority { get; set; }
        public int LowPriority { get; set; }

        public Dictionary<string, int> ByCourse { get; set; } = new();

        public double AvgEffort { get; set; }
    }
}
