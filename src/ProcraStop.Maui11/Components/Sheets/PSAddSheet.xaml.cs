using ProcraStop.Maui11.ViewModels.Common;

namespace ProcraStop.Maui11.Components.Sheets;

/// <summary>
/// Overlay "Agregar" embebido en la página (full width garantizado).
/// El anfitrión lo declara dentro de su Grid (último hijo, sobre todo lo
/// demás) y lo controla con <c>BindingContext="{Binding AddSheet}"</c> +
/// <c>IsOpen="{Binding AddSheet.IsOpen}"</c>.
/// Motion (skill procra-uiux → design-system.md §8): slide-up 250 ms
/// CubicOut / slide-down 150 ms CubeIn, con try/finally que garantiza el
/// estado final y scrim con fade. Mientras está abierto oculta TabBar y
/// NavBar de la página anfitriona (look modal completo).
/// </summary>
public partial class PSAddSheet : ContentView
{
    public static readonly BindableProperty IsOpenProperty =
        BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(PSAddSheet), false,
            propertyChanged: OnIsOpenChanged);

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public PSAddSheet()
    {
        InitializeComponent();
        // IsOpen vive en el AddSheetViewModel del BindingContext (el anfitrión
        // fija BindingContext="{Binding AddSheet}"). En code-behind porque
        // XamlC no resuelve propiedades del propio code-behind en la raíz.
        SetBinding(IsOpenProperty, new Binding(nameof(AddSheetViewModel.IsOpen)));
    }

    private static async void OnIsOpenChanged(BindableObject d, object oldValue, object newValue)
    {
        if (d is PSAddSheet sheet && newValue is bool open && oldValue is bool wasOpen && open != wasOpen)
            await sheet.AnimateAsync(open);
    }

    private async Task AnimateAsync(bool open)
    {
        // Look modal: la página anfitriona pierde TabBar y NavBar mientras
        // el sheet está abierto (el scrim cubre toda la pantalla).
        if (FindHostPage() is { } page)
        {
            Shell.SetTabBarIsVisible(page, !open);
            Shell.SetNavBarIsVisible(page, !open);
        }

        if (open)
        {
            IsVisible = true;
            try
            {
                await Task.WhenAll(
                    Scrim.FadeToAsync(1, 250),
                    SheetPanel.TranslateToAsync(0, 0, 250, Easing.CubicOut));
            }
            finally
            {
                // Nunca dejar estados a medias (lección del bug de
                // Dashboard en la app .NET 10).
                Scrim.Opacity = 1;
                SheetPanel.TranslationY = 0;
            }
        }
        else
        {
            try
            {
                await Task.WhenAll(
                    Scrim.FadeToAsync(0, 150),
                    SheetPanel.TranslateToAsync(0, 460, 150, Easing.CubicIn));
            }
            finally
            {
                Scrim.Opacity = 0;
                SheetPanel.TranslationY = 460;
                IsVisible = false;
            }
        }
    }

    private ContentPage? FindHostPage()
    {
        Element? element = this;
        while (element is not null && element is not ContentPage)
            element = element.Parent;
        return element as ContentPage;
    }
}
