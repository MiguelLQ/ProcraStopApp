using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Platform;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using ProcraStop.Modules.Tasks.Presentation.ViewModels;

namespace ProcraStop.Modules.Tasks.Presentation.Views;

public partial class CreateTaskPage : ContentPage
{
    private readonly CreateTaskPageViewModel _viewModel;

    public CreateTaskPage(CreateTaskPageViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;

        _viewModel.TaskCreated += OnTaskCreated;
    }

    private async void OnTaskCreated()
    {
        await Shell.Current.GoToAsync("..");
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
        var request = new NotificationRequest
        {
            NotificationId = 1000,
            Title = "⏰ Programada",
            Description = "Esto debería salir en 1 minuto",
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now.AddMinutes(1)

            }
        };
        

        await LocalNotificationCenter.Current.Show(request);
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