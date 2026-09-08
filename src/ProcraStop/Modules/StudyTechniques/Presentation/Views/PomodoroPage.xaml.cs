using ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;

namespace ProcraStop.Modules.StudyTechniques.Presentation.Views;

public partial class PomodoroPage : ContentPage
{
    private readonly PomodoroViewModel _vm;
    private bool _animating;

    public PomodoroPage(PomodoroViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
        _vm.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName == nameof(PomodoroViewModel.IsRunning))
                await UpdatePulseAsync();
        };
    }

    private async Task UpdatePulseAsync()
    {
        if (_vm.IsRunning && !_animating)
        {
            _animating = true;
            while (_vm.IsRunning)
            {
                await TimerSurface.ScaleToAsync(1.025, 650, Easing.SinInOut);
                await TimerSurface.ScaleToAsync(1.0, 650, Easing.SinInOut);
            }
            _animating = false;
        }
        else
        {
            await TimerSurface.ScaleToAsync(1.0, 180, Easing.SinOut);
        }
    }
}
