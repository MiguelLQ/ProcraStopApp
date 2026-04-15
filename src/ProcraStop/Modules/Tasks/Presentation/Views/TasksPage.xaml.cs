using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Platform;
using ProcraStop.Modules.Tasks.Presentation.ViewModels;

namespace ProcraStop.Modules.Tasks.Presentation.Views;

public partial class TasksPage : BasePage 
{
    protected override Color StatusBarColor => Colors.White;
    protected override StatusBarStyle StatusBarStyle => StatusBarStyle.DarkContent; 
    private readonly TasksPageViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public TasksPage(TasksPageViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();  
        BindingContext = _viewModel = viewModel;
        _serviceProvider = serviceProvider;
    }

    protected override async void OnAppearing() 
    {
        base.OnAppearing();

        await _viewModel.LoadAsync();
    }
    private async void OnAddTaskClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CreateTaskPage));
    }

}