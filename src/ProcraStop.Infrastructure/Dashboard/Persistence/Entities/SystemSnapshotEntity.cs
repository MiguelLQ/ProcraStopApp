namespace ProcraStop.Infrastructure.Dashboard.Persistence.Entities
{
    public class SystemSnapshotEntity
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }

        public double Stress { get; set; }
        public double Performance { get; set; }
        public double Behavior { get; set; }
    }
}
