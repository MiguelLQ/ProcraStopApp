using ProcraStop.Modules.Dashboard.Presentation.ViewModels;
using ProcraStop.Modules.Stress.Presentation.ViewModel;
using ProcraStop.Shared.UI;

namespace ProcraStop.Modules.Dashboard.Presentation.Views;

public partial class DashboardView : ContentPage
{
    private readonly DashboardViewModel _vm;

    public DashboardView(DashboardViewModel vm, StressCheckInViewModel stressCheckIn)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;

        // El check-in de estrés es un ContentView instanciado por XAML, por lo
        // que no puede recibir DI por constructor. La página, que sí la recibe,
        // le entrega su ViewModel de forma explícita. Antes se resolvía con
        // App.Current.Handler en el constructor, que puede ser null y provocaba
        // NullReferenceException al entrar en Inicio.
        StressCheckIn.BindingContext = stressCheckIn;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await SafeUi.RunAsync(_vm.LoadAsync, "DashboardView.Load");

        // La animación de entrada solo se ejecuta con handler nativo y siempre
        // garantiza que la página termine visible: si FadeToAsync se interrumpe
        // con Opacity=0, el usuario ve la pestaña "rota" en blanco.
        if (Handler is null)
            return;

        try
        {
            Opacity = 0;
            Scale = 0.98;

            await this.FadeToAsync(1, 250, Easing.CubicOut);
            await this.ScaleToAsync(1, 200, Easing.CubicOut);
        }
        catch (Exception exception)
        {
            ProcraStop.Diagnostics.AppCrashLogger.Log(exception, "DashboardView.EntranceAnimation");
        }
        finally
        {
            if (Handler is not null)
            {
                Opacity = 1;
                Scale = 1;
            }
        }
    }
}
