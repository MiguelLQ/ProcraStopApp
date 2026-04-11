using ProcraStop.Modules.Tasks.Presentation.ViewModels;

namespace ProcraStop.Modules.Tasks.Presentation.Views;

public partial class TasksPage : ContentPage
{
    private readonly TasksPageViewModel _viewModel;

    public TasksPage(TasksPageViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
       
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