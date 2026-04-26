namespace ProcraStop.Modules.Dashboard.Application.Models.Historial
{
    public class SystemSnapshot
    {
        public DateTime Timestamp { get; set; }

        public double Stress { get; set; }
        public double Performance { get; set; }
        public double Behavior { get; set; }
    }
}
