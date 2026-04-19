
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.DTOs
{
    public class DashboardGuidanceDto
    {
        public string Message { get; set; } = string.Empty;
        public GuidanceType Type { get; set; }
    }
}
