using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.Application.Tasks.Analytics
{
    // Metrics by course for D1 "postergacion al ultimo momento".
    public sealed class CourseProcrastinationMetricDto
    {
        public const int MinSampleSize = 3;

        public Guid? CourseId { get; init; }
        public string CourseName { get; init; } = "Sin curso";
        public int? InterestLevel { get; init; }
        public int? DifficultyLevel { get; init; }

        public int TotalTasks { get; init; }
        public int TotalReadings { get; init; }
        public int MeasuredStarts { get; init; }
        public int MeasuredDueStarts { get; init; }

        public double? AvgStartDelayDays { get; init; }
        public double OnTimeStartRate { get; init; }
        public double LastMinuteStartRate { get; init; }
        public int OverdueStartNotStarted { get; init; }
        public double? AvgLeadTimeDays { get; init; }
        public double? AvgCompletionMarginDays { get; init; }

        public bool HasAppraisal => InterestLevel.HasValue || DifficultyLevel.HasValue;
        public bool HasEnoughData =>
            MeasuredStarts >= MinSampleSize || MeasuredDueStarts >= MinSampleSize;
    }
}
