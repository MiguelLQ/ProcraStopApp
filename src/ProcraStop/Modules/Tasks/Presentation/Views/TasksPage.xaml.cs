using CommunityToolkit.Maui.Core;
using ProcraStop.Diagnostics;
using ProcraStop.Modules.Tasks.Presentation.ViewModels;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI;
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
        InitializeSnackbar(snackbarService, SnackbarHost);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await SafeUi.RunAsync(async () =>
        {
            await _viewModel.LoadAsync();

            // Consume el mensaje; si no hay nada, retorna null.
            var (msg, type) = PendingNotificationService.Consume();

            if (!string.IsNullOrWhiteSpace(msg))
                _ = ShowPendingSnackbarAsync(msg, type);
        }, "TasksPage.OnAppearing");
    }

    private async Task ShowPendingSnackbarAsync(string msg, SnackbarType type)
    {
        try
        {
            await Task.Delay(1500);
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                Console.WriteLine($"[TasksPage] Disparando snackbar: '{msg}'");
                await ShowSnackbarAsync(msg, type);
            });
        }
        catch (Exception exception)
        {
            AppCrashLogger.Log(exception, "TasksPage.ShowPendingSnackbarAsync");
        }
    }

    private async void OnAddTaskTapped(object? sender, TappedEventArgs e)
    {
        if (_isNavigatingToCreate)
            return;

        try
        {
            _isNavigatingToCreate = true;
            await ShellNavigation.GoToAsync(nameof(CreateTaskPage));
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
