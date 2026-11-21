using ProcraStop.Maui11.ViewModels.Tasks;

namespace ProcraStop.Maui11.Views.Tasks;

public partial class TaskEditorPage : ContentPage, IQueryAttributable
{
    private readonly TaskEditorViewModel _viewModel;

    public TaskEditorPage(TaskEditorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _viewModel.ApplyQueryAttributes(query);

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
