using ProcraStop.Modules.Stress.Presentation.ViewModel;

namespace ProcraStop.Modules.Stress.Presentation.Views;

public partial class StressAnalyticsPage : ContentPage
{
    private readonly StressAnalyticsViewModel _vm;

    public StressAnalyticsPage(StressAnalyticsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}