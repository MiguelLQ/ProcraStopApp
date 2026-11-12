using ProcraStop.Modules.Stress.Presentation.ViewModel;
using ProcraStop.Shared.UI;

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
        await SafeUi.RunAsync(_vm.LoadAsync, "StressAnalyticsPage.OnAppearing");
    }
}