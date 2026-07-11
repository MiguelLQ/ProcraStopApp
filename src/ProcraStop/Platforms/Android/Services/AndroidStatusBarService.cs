using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;

namespace ProcraStop.Platforms.Android.Services;

internal static class AndroidStatusBarService
{
    private const string LightStatusBarColor = "#FFFFFF";
    private const string DarkStatusBarColor = "#171B24";
    private const int StatusBarProtectionViewId = 0x50535001;

    public static void ApplyThemeStatusBar(Activity? activity)
    {
        if (activity?.Window is not { } window)
            return;

        var isDarkTheme = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark;
        var statusBarColor = global::Android.Graphics.Color.ParseColor(isDarkTheme ? DarkStatusBarColor : LightStatusBarColor);

        window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
        window.ClearFlags(WindowManagerFlags.TranslucentStatus);
        WindowCompat.SetDecorFitsSystemWindows(window, true);

#pragma warning disable CA1422
        window.SetStatusBarColor(statusBarColor);
#pragma warning restore CA1422

        var controller = WindowCompat.GetInsetsController(window, window.DecorView);
        if (controller is not null)
            controller.AppearanceLightStatusBars = !isDarkTheme;

        if ((int)Build.VERSION.SdkInt >= 35)
            ApplyAndroid15StatusBarProtection(window, statusBarColor);
        else
            RemoveAndroid15StatusBarProtection(window);
    }

    private static void ApplyAndroid15StatusBarProtection(global::Android.Views.Window window, global::Android.Graphics.Color color)
    {
        if (window.DecorView is not FrameLayout decorView)
            return;

        var protectionView = decorView.FindViewById(StatusBarProtectionViewId);
        if (protectionView is null)
        {
            protectionView = new global::Android.Views.View(window.Context)
            {
                Id = StatusBarProtectionViewId,
                ImportantForAccessibility = ImportantForAccessibility.No
            };

            decorView.AddView(
                protectionView,
                new FrameLayout.LayoutParams(
                    FrameLayout.LayoutParams.MatchParent,
                    GetStatusBarHeight(window),
                    GravityFlags.Top));
        }

        protectionView.SetBackgroundColor(color);
        UpdateProtectionHeight(window, protectionView);
        protectionView.BringToFront();
    }

    private static void RemoveAndroid15StatusBarProtection(global::Android.Views.Window window)
    {
        if (window.DecorView is not FrameLayout decorView)
            return;

        var protectionView = decorView.FindViewById(StatusBarProtectionViewId);
        if (protectionView is not null)
            decorView.RemoveView(protectionView);
    }

    private static void UpdateProtectionHeight(global::Android.Views.Window window, global::Android.Views.View protectionView)
    {
        var height = GetStatusBarHeight(window);
        if (protectionView.LayoutParameters is FrameLayout.LayoutParams layoutParams)
        {
            layoutParams.Width = FrameLayout.LayoutParams.MatchParent;
            layoutParams.Height = height;
            layoutParams.Gravity = GravityFlags.Top;
            protectionView.LayoutParameters = layoutParams;
        }
    }

    private static int GetStatusBarHeight(global::Android.Views.Window window)
    {
        var decorView = window.DecorView;
        if (decorView is null)
            return 24;

        var insets = ViewCompat.GetRootWindowInsets(decorView);
        var statusBarInset = insets?.GetInsets(WindowInsetsCompat.Type.StatusBars()).Top ?? 0;
        if (statusBarInset > 0)
            return statusBarInset;

        var resources = window.Context?.Resources;
        if (resources is null)
            return 24;

        var resourceId = resources.GetIdentifier("status_bar_height", "dimen", "android");
        if (resourceId > 0)
            return resources.GetDimensionPixelSize(resourceId);

        return (int)(24 * (resources.DisplayMetrics?.Density ?? 1));
    }
}
