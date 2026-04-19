using ProcraStop.Modules.Tasks.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcraStop.Modules.Tasks.Application.DTOs
{
    public class RecommendedTaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? EstimatedMinutes { get; set; }
        public TaskEffort Effort { get; set; }
    }
}
