using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcraStop.Modules.Tasks.Presentation.Models
{
    public partial class SubtaskModel : ObservableObject
    {
        public Guid Id { get; set; }

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private bool isCompleted;
    }
}
