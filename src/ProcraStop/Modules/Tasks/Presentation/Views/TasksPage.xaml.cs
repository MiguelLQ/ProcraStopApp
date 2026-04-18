using CommunityToolkit.Maui.Core;
using ProcraStop.Modules.Tasks.Presentation.ViewModels;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI.Components;
using ProcraStop.Shared.UI.Layouts;

namespace ProcraStop.Modules.Tasks.Presentation.Views;

public partial class TasksPage : BasePage
{
    protected override Color StatusBarColor => Colors.White;
    protected override StatusBarStyle StatusBarStyle => StatusBarStyle.DarkContent;
    private readonly TasksPageViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public TasksPage(TasksPageViewModel viewModel, IServiceProvider serviceProvider, ISnackbarService snackbarService)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        InitializeSnackbar(snackbarService);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();

        // Consume el mensaje — si no hay nada, retorna null
        var (msg, type) = PendingNotificationService.Consume();

        if (!string.IsNullOrWhiteSpace(msg))
            _ = ShowPendingSnackbarAsync(msg, type);
    }

    private async Task ShowPendingSnackbarAsync(string msg, SnackbarType type)
    {
        await Task.Delay(1500);
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            Console.WriteLine($"[TasksPage] Disparando snackbar: '{msg}'");
            await Snackbar.ShowAsync(msg, type);
        });
    }

    private async void OnAddTaskClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CreateTaskPage));
    }
}