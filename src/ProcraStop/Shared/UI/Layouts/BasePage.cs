using Microsoft.Maui.Layouts;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI.Components;

namespace ProcraStop.Shared.UI.Layouts
{
    public class BasePage : ContentPage
    {
        protected ISnackbarService? SnackbarService;
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
            SubscribeSnackbar();
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
