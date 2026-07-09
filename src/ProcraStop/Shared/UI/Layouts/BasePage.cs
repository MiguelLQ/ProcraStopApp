using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Platform;
using Microsoft.Maui.Layouts;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI.Components;
#if ANDROID
using Microsoft.Maui.Platform;
#endif

namespace ProcraStop.Shared.UI.Layouts
{
    public class BasePage : ContentPage
    {
        protected ISnackbarService? SnackbarService;
        protected virtual Color StatusBarColor => GetPrimaryColor(); // editbycodex: color primario común para páginas base.
        protected virtual StatusBarStyle StatusBarStyle => StatusBarStyle.LightContent;
        protected readonly CustomSnackbar Snackbar;
        private bool _isSnackbarSubscribed; // editbycodex: controla el ciclo de vida sin listeners duplicados.

        public BasePage()
        {
            Snackbar = new CustomSnackbar
            {
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.Fill,
                Margin = new Thickness(0, 0, 0, 0),
                IsVisible = false,
                ZIndex = 99
            };
        }

        // Se llama cuando la Page ya tiene handler nativo asignado
        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            if (Handler is null) return;
            ApplyStatusBar(); // editbycodex: Maui 10 necesita aplicar cuando la ventana nativa ya existe.

            if (Content is View pageContent && !(Content is AbsoluteLayout al && al.Children.Contains(Snackbar)))
            {
                // Envuelve el contenido en un AbsoluteLayout
                var wrapper = new AbsoluteLayout();

                // Quita el contenido actual
                Content = null;

                // Contenido ocupa todo el espacio
                AbsoluteLayout.SetLayoutBounds(pageContent, new Rect(0, 0, 1, 1));
                AbsoluteLayout.SetLayoutFlags(pageContent, AbsoluteLayoutFlags.All);
                wrapper.Add(pageContent);

                // Snackbar flotante abajo, sin afectar el layout
                AbsoluteLayout.SetLayoutBounds(Snackbar, new Rect(0, 1, 1, AbsoluteLayout.AutoSize));
                AbsoluteLayout.SetLayoutFlags(Snackbar, AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional);
                Snackbar.Margin = new Thickness(0, 0, 0, 40);
                wrapper.Add(Snackbar);

                Content = wrapper;

                Console.WriteLine($"[BasePage] Wrapper AbsoluteLayout creado. Handler={Snackbar.Handler is not null}");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ApplyStatusBar();
            Dispatcher.DispatchDelayed(
                TimeSpan.FromMilliseconds(80),
                ApplyStatusBar); // editbycodex: segunda aplicación tras completar la navegación Shell.
            SubscribeSnackbar();
        }

        private void ApplyStatusBar()
        {
            var color = StatusBarColor;
            StatusBar.SetColor(color);
            StatusBar.SetStyle(StatusBarStyle);

#if ANDROID
            // editbycodex: respaldo nativo para versiones donde Toolkit pierde el color durante Shell navigation.
            if (Handler?.MauiContext?.Context is Android.App.Activity activity &&
                activity.Window is Android.Views.Window window)
            {
                window.AddFlags(Android.Views.WindowManagerFlags.DrawsSystemBarBackgrounds);
                window.ClearFlags(Android.Views.WindowManagerFlags.TranslucentStatus);
#pragma warning disable CA1422
                window.SetStatusBarColor(color.ToPlatform());
#pragma warning restore CA1422
                var controller = AndroidX.Core.View.WindowCompat.GetInsetsController(window, window.DecorView);
                controller.AppearanceLightStatusBars = StatusBarStyle == StatusBarStyle.DarkContent;
            }
#endif
        }

        private static Color GetPrimaryColor()
        {
            if (Microsoft.Maui.Controls.Application.Current?.Resources.TryGetValue("primarioS", out var resource) == true)
            {
                if (resource is Color color)
                    return color;
                if (resource is SolidColorBrush brush)
                    return brush.Color;
            }

            return Color.FromArgb("#4075FF");
        }

        protected void InitializeSnackbar(ISnackbarService snackbarService)
        {
            SnackbarService = snackbarService;
            SubscribeSnackbar();
            Console.WriteLine($"[BasePage] Snackbar suscrito OK");
        }

        private void SubscribeSnackbar()
        {
            if (SnackbarService is null || _isSnackbarSubscribed)
                return;

            SnackbarService.OnShowRequested += Snackbar.ShowAsync;
            _isSnackbarSubscribed = true; // editbycodex: recupera feedback al volver de navegación.
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (SnackbarService is not null)
            {
                SnackbarService.OnShowRequested -= Snackbar.ShowAsync;
                _isSnackbarSubscribed = false;
            }
        }
    }
}
