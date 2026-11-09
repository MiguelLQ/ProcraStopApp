using CommunityToolkit.Maui.Core;
using ProcraStop.Modules.Tasks.Presentation.ViewModels;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI.Components;
using ProcraStop.Shared.UI.Layouts;

namespace ProcraStop.Modules.Tasks.Presentation.Views;

public partial class TasksPage : BasePage
{
    // editbycodex: usa el status bar primario definido por BasePage.
    private readonly TasksPageViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;
    private bool _isNavigatingToCreate;

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

    private async void OnAddTaskTapped(object? sender, TappedEventArgs e)
    {
        if (_isNavigatingToCreate)
            return;

        try
        {
            _isNavigatingToCreate = true;
            await Shell.Current.GoToAsync(nameof(CreateTaskPage));
        }
        finally
        {
            _isNavigatingToCreate = false;
        }
    }

    private void OnSearchButtonTapped(object? sender, TappedEventArgs e)
    {
        _viewModel.ToggleSearchPanelCommand.Execute(null);
        Dispatcher.DispatchDelayed(
            TimeSpan.FromMilliseconds(100),
            () => SearchInput.Focus());
    }
}
