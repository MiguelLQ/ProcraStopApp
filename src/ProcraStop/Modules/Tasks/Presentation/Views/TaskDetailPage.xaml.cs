using ProcraStop.Modules.Tasks.Presentation.ViewModels;
using Microsoft.Maui.Controls;
using System;

namespace ProcraStop.Modules.Tasks.Presentation.Views;

public partial class TaskDetailPage : ContentPage
{
    public TaskDetailPage(TaskDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
