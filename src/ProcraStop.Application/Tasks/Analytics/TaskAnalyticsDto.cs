

namespace ProcraStop.Application.Tasks.Analytics
{
    public class TaskAnalyticsDto
    {
        public int Total { get; set; }
        public int Completed { get; set; }
        public int Pending { get; set; }
        public int InProgress { get; set; }
        public int Overdue { get; set; }

        public int HighPriority { get; set; }
        public int MediumPriority { get; set; }
        public int LowPriority { get; set; }

        // D1 EPA: postergacion al ultimo momento.
        public double? AvgHoursToFirstProgress { get; set; }
        public double? AvgStartDelayDays { get; set; }
        public double OnTimeStartRate { get; set; }
        public double LastMinuteStartRate { get; set; }
        public int OverdueStartNotStarted { get; set; }
        public int MeasuredStarts { get; set; }
        public int MeasuredDueStarts { get; set; }

        public Dictionary<string, int> ByCourse { get; set; } = new();

        public double AvgEffort { get; set; }
        public double CompletionRate { get; set; }
        public int CompletedThisWeek { get; set; }
        public int DueNextSevenDays { get; set; }
        public string[] WeeklyLabels { get; set; } = Array.Empty<string>();
        public double[] WeeklyCompleted { get; set; } = Array.Empty<double>();
        public string[] StatusLabels { get; set; } = Array.Empty<string>();
        public double[] StatusValues { get; set; } = Array.Empty<double>();
        public string[] PriorityLabels { get; set; } = Array.Empty<string>();
        public double[] PriorityValues { get; set; } = Array.Empty<double>();
        public string[] CourseLabels { get; set; } = Array.Empty<string>();
        public double[] CourseValues { get; set; } = Array.Empty<double>();
    }
}
