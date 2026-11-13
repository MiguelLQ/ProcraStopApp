using ProcraStop.Maui11.ViewModels.Tasks;

namespace ProcraStop.Maui11.Views.Tasks;

public partial class TasksPage : ContentPage
{
    public TasksPage(TasksPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
