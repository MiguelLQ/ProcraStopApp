using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Platform;
using Microsoft.Maui.Layouts;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI.Components;

namespace ProcraStop.Shared.UI.Layouts
{
    public class BasePage : ContentPage
    {
        protected ISnackbarService? SnackbarService;
        protected virtual Color StatusBarColor => Colors.White;
        protected virtual StatusBarStyle StatusBarStyle => StatusBarStyle.DarkContent;
        protected readonly CustomSnackbar Snackbar;

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
            StatusBar.SetColor(StatusBarColor);
            StatusBar.SetStyle(StatusBarStyle);
        }

        protected void InitializeSnackbar(ISnackbarService snackbarService)
        {
            SnackbarService = snackbarService;
            SnackbarService.OnShowRequested += Snackbar.ShowAsync;
            Console.WriteLine($"[BasePage] Snackbar suscrito OK");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (SnackbarService is not null)
                SnackbarService.OnShowRequested -= Snackbar.ShowAsync;
        }
    }
}