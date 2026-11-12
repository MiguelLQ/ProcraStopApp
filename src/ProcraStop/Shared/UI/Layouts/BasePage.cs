using ProcraStop.Diagnostics;
using ProcraStop.Shared.Services;
using ProcraStop.Shared.UI.Components;

namespace ProcraStop.Shared.UI.Layouts
{
    /// <summary>
    /// Base de las páginas que muestran el snackbar de confirmación.
    ///
    /// IMPORTANTE: el control <see cref="CustomSnackbar"/> se declara en el
    /// XAML de cada página (no se inyecta en runtime). Antes se envolvía el
    /// contenido de la página en un <see cref="AbsoluteLayout"/> desde
    /// <c>OnHandlerChanged</c>, lo que obligaba a hacer <c>Content = null</c>:
    /// al desconectar el contenido de la página se destruía el handler de los
    /// <c>CollectionView</c> y MAUI volvía a inflar sus plantillas mientras
    /// estaban desconectadas, con el <c>BindingContext</c> heredado en
    /// <c>null</c>. En ese momento los bindings compilados con
    /// <c>Source={RelativeSource AncestorType=...}</c> se aplicaban con
    /// <c>source == null</c> y MAUI lanzaba
    /// <c>NullReferenceException</c> al entrar y salir de las pantallas.
    /// </summary>
    public class BasePage : ContentPage
    {
        protected ISnackbarService? SnackbarService { get; private set; }

        protected CustomSnackbar? Snackbar { get; private set; }

        private bool _isSnackbarSubscribed; // editbycodex: controla el ciclo de vida sin listeners duplicados.

        protected override void OnAppearing()
        {
            base.OnAppearing();
            SubscribeSnackbar();
        }

        /// <summary>
        /// Registra el servicio de snackbar y el control declarado en el XAML.
        /// </summary>
        protected void InitializeSnackbar(ISnackbarService snackbarService, CustomSnackbar snackbar)
        {
            SnackbarService = snackbarService;
            Snackbar = snackbar;
            SubscribeSnackbar();
        }

        /// <summary>
        /// Muestra un mensaje en el snackbar de la página. Si la página no
        /// declara el control, la operación se ignora sin lanzar excepción.
        /// </summary>
        protected Task ShowSnackbarAsync(string message, SnackbarType type)
            => Snackbar is null ? Task.CompletedTask : Snackbar.ShowAsync(message, type);

        private void SubscribeSnackbar()
        {
            if (SnackbarService is null || Snackbar is null || _isSnackbarSubscribed)
                return;

            SnackbarService.OnShowRequested += Snackbar.ShowAsync;
            _isSnackbarSubscribed = true; // editbycodex: recupera feedback al volver de navegación.
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Evita que una animación de snackbar siga viva cuando el handler
            // nativo ya se desconectó.
            Snackbar?.HideNow();

            if (SnackbarService is not null && Snackbar is not null)
            {
                SnackbarService.OnShowRequested -= Snackbar.ShowAsync;
                _isSnackbarSubscribed = false;
            }
        }
    }
}
