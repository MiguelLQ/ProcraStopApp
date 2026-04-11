
using ProcraStop.Modules.Tasks.Presentation.ViewModels;

namespace ProcraStop.Modules.Tasks.Presentation.Views;

public partial class TaskListPage : ContentPage
{
    private readonly TaskListViewModel _pageModel;

    public TaskListPage(TaskListViewModel pageModel)
    {
        InitializeComponent();
        _pageModel = pageModel;
        BindingContext = _pageModel;
    }

    //protected override void OnAppearing()
    //{
    //    base.OnAppearing();

    //    // Ocultar el encabezado del Shell
    //    Shell.SetNavBarIsVisible(this, false);
    //}
    //protected override async void OnAppearing()
    //{

    //    Shell.SetBackgroundColor(this, Color.FromArgb("#4B5BDB"));
    //    Shell.SetForegroundColor(this, Colors.White);
    //}

}