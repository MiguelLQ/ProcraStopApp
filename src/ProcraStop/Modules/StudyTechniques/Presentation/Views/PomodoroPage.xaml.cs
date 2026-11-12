using System.ComponentModel;
using ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;
using ProcraStop.Shared.UI;

namespace ProcraStop.Modules.StudyTechniques.Presentation.Views;

public partial class PomodoroPage : ContentPage
{
    private readonly PomodoroViewModel _vm;
    private CancellationTokenSource? _pulseCts;
    private bool _animating;

    public PomodoroPage(PomodoroViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
        _vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(PomodoroViewModel.IsRunning))
            return;

        // El handler original era una lambda async void: cualquier fallo de la
        // animación se convertía en excepción no observada y cerraba la app.
        await SafeUi.RunAsync(UpdatePulseAsync, "PomodoroPage.UpdatePulse");
    }

    private async Task UpdatePulseAsync()
    {
        if (!_vm.IsRunning)
        {
            StopPulse();

            if (Handler is not null)
                await TimerSurface.ScaleToAsync(1.0, 180, Easing.SinOut);

            return;
        }

        if (_animating)
            return;

        _animating = true;
        _pulseCts = new CancellationTokenSource();
        var token = _pulseCts.Token;

        try
        {
            while (_vm.IsRunning && !token.IsCancellationRequested && Handler is not null)
            {
                await TimerSurface.ScaleToAsync(1.025, 650, Easing.SinInOut);
                await TimerSurface.ScaleToAsync(1.0, 650, Easing.SinInOut);
            }
        }
        catch (OperationCanceledException)
        {
            // La página se cerró o el temporizador se detuvo.
        }
        finally
        {
            _animating = false;

            _pulseCts?.Dispose();
            _pulseCts = null;

            if (Handler is not null)
                TimerSurface.Scale = 1.0;
        }
    }

    private void StopPulse()
    {
        try
        {
            _pulseCts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Ya estaba liberado.
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // No dejar animando un control cuyo handler ya se desconectó.
        StopPulse();
    }
}
