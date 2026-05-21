

namespace ProcraStop.Domain.Stress.ValueObject
{
    public readonly struct StressLevel
    {
        public double Value { get; }

        public StressLevel(double value)
        {
            Value = Math.Clamp(value, 0, 1);
        }

        public static implicit operator double(StressLevel s) => s.Value;
    }
}
