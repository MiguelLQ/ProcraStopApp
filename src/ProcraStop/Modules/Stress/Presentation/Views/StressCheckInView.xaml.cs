using ProcraStop.Diagnostics;
using ProcraStop.Modules.Stress.Presentation.ViewModel;

namespace ProcraStop.Modules.Stress.Presentation.Views;

public partial class StressCheckInView : ContentView
{
    private bool _initialized;

    public StressCheckInView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        // Loaded puede dispararse más de una vez (re-entrada a la pestaña).
        if (_initialized)
            return;

        if (BindingContext is not StressCheckInViewModel viewModel)
        {
            // El ViewModel lo inyecta la página contenedora (DashboardView).
            // Si no está, se oculta en lugar de lanzar NullReferenceException.
            IsVisible = false;
            return;
        }

        _initialized = true;

        try
        {
            await viewModel.InitializeAsync();
        }
        catch (Exception exception)
        {
            AppCrashLogger.Log(exception, "StressCheckInView.OnLoaded");
        }
    }
}
