using ProcraStop.Modules.Academic.Presentation.ViewModels;
using ProcraStop.Shared.UI;

namespace ProcraStop.Modules.Academic.Presentation.Views;

public partial class SchedulePage : ContentPage
{
    private readonly SchedulePageViewModel _vm;

    public SchedulePage(SchedulePageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await SafeUi.RunAsync(_vm.LoadAsync, "SchedulePage.OnAppearing");
    }
}
