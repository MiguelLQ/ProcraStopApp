using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Modules.Tasks.Presentation.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ProcraStop.Modules.Tasks.Presentation.ViewModels
{
    public partial class TaskListViewModel : ObservableObject
    {
        [RelayCommand]
        public async Task IrAAgregarTareaAsync()
        {
            // Usa la ruta definida en AppShell.xaml
            await Shell.Current.GoToAsync("AddTaskPage");
        }
    }
}
