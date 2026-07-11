namespace ProcraStop.Modules.Stress.Presentation.Messages;

public sealed class StressCheckInSubmittedMessage
{
    public static StressCheckInSubmittedMessage Instance { get; } = new();

    private StressCheckInSubmittedMessage()
    {
    }
}
