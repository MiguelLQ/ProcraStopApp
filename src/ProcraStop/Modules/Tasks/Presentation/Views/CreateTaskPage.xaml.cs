using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Platform;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using ProcraStop.Diagnostics;
using ProcraStop.Modules.Tasks.Presentation.ViewModels;
using ProcraStop.Shared.Services;

namespace ProcraStop.Modules.Tasks.Presentation.Views;

public partial class CreateTaskPage : ContentPage
{
    private readonly CreateTaskPageViewModel _viewModel;
    private bool _isReturningToTasks;

    public CreateTaskPage(CreateTaskPageViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async Task OnTaskCreated()
    {
        if (_isReturningToTasks)
            return;

        try
        {
            _isReturningToTasks = true;
            await ShellNavigation.GoBackAsync();
        }
        catch (Exception ex)
        {
            AppCrashLogger.Log(ex, "CreateTaskPage.OnTaskCreated");
        }
        finally
        {
            _isReturningToTasks = false;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        try
        {
            _viewModel.TaskCreated -= OnTaskCreated;
        }
        catch (Exception ex)
        {
            AppCrashLogger.Log(ex, "CreateTaskPage.OnDisappearing");
        }
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            // La suscripción se hace aquí (no en el constructor) para que la
            // página vuelva a recibir el evento si regresa a primer plano.
            _viewModel.TaskCreated -= OnTaskCreated;
            _viewModel.TaskCreated += OnTaskCreated;

            if (BindingContext is CreateTaskPageViewModel vm)
                await vm.LoadAsync();
        }
        catch (Exception ex)
        {
            AppCrashLogger.Log(ex, "CreateTaskPage.OnAppearing");
        }
        await RequestNotificationPermissionAsync();
    }
    public async Task RequestNotificationPermissionAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();

            if (status != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.PostNotifications>();
            }
        }
        catch (Exception ex)
        {
            AppCrashLogger.Log(ex, "CreateTaskPage.RequestNotificationPermission");
        }
    }
    private void OnSubtaskCompleted(object sender, EventArgs e)
    {
        try
        {
            if (BindingContext is CreateTaskPageViewModel vm)
                vm.AddSubtaskCommand.Execute(null);
        }
        catch (Exception ex)
        {
            AppCrashLogger.Log(ex, "CreateTaskPage.OnSubtaskCompleted");
        }
    }
}
