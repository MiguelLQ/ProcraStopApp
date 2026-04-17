using ProcraStop.Shared.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcraStop.Shared.Services
{
    public interface ISnackbarService
    {
        Task ShowAsync(string message, SnackbarType type = SnackbarType.Success);
        // ver mensaje de error
        Task ShowErrorAsync(string message, SnackbarType type = SnackbarType.Error);

    }
}
