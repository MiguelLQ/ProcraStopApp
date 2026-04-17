using ProcraStop.Shared.UI.Components;


namespace ProcraStop.Shared.Helpers
{
    public static class CustomSnackbarExtensions
    {
        public static (string icon, Color color) GetStyle(this SnackbarType type)
        {
            return type switch
            {
                SnackbarType.Success => ("✔", Color.FromArgb("#46c872")),
                SnackbarType.Error => ("✖", Color.FromArgb("#DC2626")),
                SnackbarType.Warning => ("⚠", Color.FromArgb("#F59E0B")),
                SnackbarType.Info => ("ℹ", Color.FromArgb("#2563EB")),
                _ => ("✔", Color.FromArgb("#1F2937"))
            };
        }
    }
}
