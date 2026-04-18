using ProcraStop.Shared.UI.Components;


namespace ProcraStop.Shared.Services
{
    public class SnackbarService : ISnackbarService
    {
        public event Func<string, SnackbarType, Task>? OnShowRequested;

        public async Task ShowAsync(string message, SnackbarType type = SnackbarType.Success)
        {
            Console.WriteLine($"[SnackbarService] ShowAsync - listeners={OnShowRequested?.GetInvocationList().Length ?? 0}");

            if (OnShowRequested is not null)
                await OnShowRequested.Invoke(message, type);
        }

        public async Task ShowErrorAsync(string message, SnackbarType type = SnackbarType.Error)
        {
            if (OnShowRequested is not null)
                await OnShowRequested.Invoke(message, type);
        }
        public async Task ShowInfoAsync(string message, SnackbarType type = SnackbarType.Info)
        {
            if (OnShowRequested is not null)
                await OnShowRequested.Invoke(message, type);
        }
    }
}
