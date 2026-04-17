using ProcraStop.Shared.UI.Components;


namespace ProcraStop.Shared.Services
{
    public class SnackbarService : ISnackbarService
    {
        public event Func<string, SnackbarType, Task>? OnShowRequested;

        public async Task ShowAsync(string message, SnackbarType type = SnackbarType.Success)
        {
            if (OnShowRequested is not null)
                await OnShowRequested.Invoke(message, type);
        }

        public async Task ShowErrorAsync(string message, SnackbarType type = SnackbarType.Error)
        {
            if (OnShowRequested is not null)
                await OnShowRequested.Invoke(message, type);
        }
    }
}
