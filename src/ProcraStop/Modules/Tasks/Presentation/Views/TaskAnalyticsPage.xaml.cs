using ProcraStop.Modules.Tasks.Presentation.ViewModels;

namespace ProcraStop.Modules.Tasks.Presentation.Views;

public partial class TaskAnalyticsPage : ContentPage
{
    private readonly TaskAnalyticsViewModel _vm;

    public TaskAnalyticsPage( TaskAnalyticsViewModel vm)
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