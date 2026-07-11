using ProcraStop.Modules.Academic.Presentation.ViewModels;

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
        await _vm.LoadAsync();
    }
}
