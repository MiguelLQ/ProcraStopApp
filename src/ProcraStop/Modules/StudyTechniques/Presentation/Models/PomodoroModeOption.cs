namespace ProcraStop.Modules.StudyTechniques.Presentation.Models;

public sealed class PomodoroModeOption
{
    public PomodoroModeOption(string name, int focusMinutes, int breakMinutes, string detail)
    {
        Name = name;
        FocusMinutes = focusMinutes;
        BreakMinutes = breakMinutes;
        Detail = detail;
    }

    public string Name { get; }
    public int FocusMinutes { get; }
    public int BreakMinutes { get; }
    public string Detail { get; }
}
