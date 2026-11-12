using ProcraStop.Modules.Tasks.Presentation.ViewModels;
using ProcraStop.Shared.UI;

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
        await SafeUi.RunAsync(_vm.LoadAsync, "TaskAnalyticsPage.OnAppearing");
    }
}