using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Platform;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using ProcraStop.Modules.Tasks.Presentation.ViewModels;

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

        _viewModel.TaskCreated += OnTaskCreated;
    }

    private async Task OnTaskCreated()
    {
        if (_isReturningToTasks || Shell.Current is not { } shell)
            return;

        try
        {
            _isReturningToTasks = true;
            await shell.GoToAsync("..");
        }
        finally
        {
            _isReturningToTasks = false;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _viewModel.TaskCreated -= OnTaskCreated;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CreateTaskPageViewModel vm)
        {
            await vm.LoadAsync();
        }
        await RequestNotificationPermissionAsync();

    }
    public async Task RequestNotificationPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();

        if (status != PermissionStatus.Granted)
        {
            await Permissions.RequestAsync<Permissions.PostNotifications>();
        }
    }
    private void OnSubtaskCompleted(object sender, EventArgs e)
    {
        if (BindingContext is CreateTaskPageViewModel vm)
            vm.AddSubtaskCommand.Execute(null);
    }
}
