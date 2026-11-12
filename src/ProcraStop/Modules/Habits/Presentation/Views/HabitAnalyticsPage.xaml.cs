using ProcraStop.Modules.Habits.Presentation.ViewModels;
using ProcraStop.Shared.UI;

namespace ProcraStop.Modules.Habits.Presentation.Views;

public partial class HabitAnalyticsPage : ContentPage
{
    private readonly HabitAnalyticsViewModel _vm;

    public HabitAnalyticsPage(HabitAnalyticsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await SafeUi.RunAsync(_vm.LoadAsync, "HabitAnalyticsPage.OnAppearing");
    }
}
