using ProcraStop.Modules.Stress.Presentation.ViewModel;

namespace ProcraStop.Modules.Stress.Presentation.Views;

public partial class StressCheckInView : ContentView
{
    private readonly StressCheckInViewModel _vm;

    public StressCheckInView()
    {
        InitializeComponent();

        _vm = App.Current?.Handler?.MauiContext?.Services
            .GetService<StressCheckInViewModel>()!;

        BindingContext = _vm;

        Loaded += async (_, _) =>
        {
            await _vm.InitializeAsync();
        };
    }
}