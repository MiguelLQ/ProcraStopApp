using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Platform;

namespace ProcraStop.Shared.UI
{
    public class BasePage : ContentPage
    {
        protected virtual Color StatusBarColor => Colors.White;
        protected virtual StatusBarStyle StatusBarStyle => StatusBarStyle.DarkContent;

        protected override void OnAppearing()
        {
            base.OnAppearing();

            StatusBar.SetColor(StatusBarColor);
            StatusBar.SetStyle(StatusBarStyle);
        }
    }
}
