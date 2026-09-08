using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Modules.StudyTechniques.Presentation.Models;
using System.Collections.ObjectModel;

namespace ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;

public partial class PomodoroViewModel : ObservableObject
{
    private readonly IDispatcherTimer? _timer;
    private int _remainingSeconds;

    public PomodoroViewModel()
    {
        Modes.Add(new PomodoroModeOption("Rescate", 10, 2, "Para vencer el arranque."));
        Modes.Add(new PomodoroModeOption("Clásico", 25, 5, "Para estudiar con ritmo estable."));
        Modes.Add(new PomodoroModeOption("Profundo", 50, 10, "Para sesiones largas y claras."));

        SelectedMode = Modes[1];
        _remainingSeconds = SelectedMode.FocusMinutes * 60;
        TimerText = FormatTime(_remainingSeconds);
        ModeDescription = SelectedMode.Detail;

        _timer = Microsoft.Maui.Controls.Application.Current?.Dispatcher.CreateTimer();
        if (_timer is not null)
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimerTick;
        }
    }

    public ObservableCollection<PomodoroModeOption> Modes { get; } = new();

    [ObservableProperty] private PomodoroModeOption? selectedMode;
    [ObservableProperty] private string timerText = "25:00";
    [ObservableProperty] private string stateText = "Listo para empezar";
    [ObservableProperty] private string modeDescription = "";
    [ObservableProperty] private bool isRunning;
    [ObservableProperty] private int completedCycles;
    [ObservableProperty] private double progress = 1;

    partial void OnSelectedModeChanged(PomodoroModeOption? value)
    {
        if (value is null || IsRunning)
            return;

        _remainingSeconds = value.FocusMinutes * 60;
        TimerText = FormatTime(_remainingSeconds);
        ModeDescription = value.Detail;
        Progress = 1;
        StateText = "Listo para empezar";
    }

    [RelayCommand]
    public void Toggle()
    {
        if (_timer is null)
            return;

        if (IsRunning)
        {
            _timer.Stop();
            IsRunning = false;
            StateText = "Pausado";
            return;
        }

        if (_remainingSeconds <= 0)
            Reset();

        _timer.Start();
        IsRunning = true;
        StateText = "En foco";
    }

    [RelayCommand]
    public void Reset()
    {
        _timer?.Stop();
        IsRunning = false;
        _remainingSeconds = (SelectedMode?.FocusMinutes ?? 25) * 60;
        TimerText = FormatTime(_remainingSeconds);
        Progress = 1;
        StateText = "Listo para empezar";
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_remainingSeconds <= 0)
        {
            _timer?.Stop();
            IsRunning = false;
            CompletedCycles++;
            Progress = 0;
            StateText = $"Descansa {SelectedMode?.BreakMinutes ?? 5} minutos";
            return;
        }

        _remainingSeconds--;
        TimerText = FormatTime(_remainingSeconds);
        var total = Math.Max(1, (SelectedMode?.FocusMinutes ?? 25) * 60);
        Progress = Math.Clamp((double)_remainingSeconds / total, 0, 1);
    }

    private static string FormatTime(int seconds)
    {
        var time = TimeSpan.FromSeconds(Math.Max(0, seconds));
        return $"{time.Minutes:00}:{time.Seconds:00}";
    }
}
