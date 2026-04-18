namespace ProcraStop.Shared.UI.Components;

using Microsoft.Maui.Controls;
using ProcraStop.Shared.Helpers;

public partial class CustomSnackbar : ContentView
{
    private TaskCompletionSource? _tcs;

    public CustomSnackbar()
    {
        InitializeComponent();

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, __) => await Hide();
        this.GestureRecognizers.Add(tap);
    }

    public async Task Show(string message, string icon, Color color)
    {
        Console.WriteLine($"[CustomSnackbar] Show - Handler={Handler is not null}, IsVisible={IsVisible}");

        Message.Text = message;
        Icon.Text = icon;
        IconContainer.BackgroundColor = color;

        IsVisible = true;

        TranslationY = 120;
        Scale = 0.9;
        Opacity = 0;

        await Task.WhenAll(
            this.TranslateToAsync(0, 0, 300, Easing.CubicOut),
            this.FadeToAsync(1, 250),
            this.ScaleToAsync(1, 250, Easing.CubicOut)
        );

        await this.ScaleToAsync(1.03, 100);
        await this.ScaleToAsync(1, 80);

        _tcs = new TaskCompletionSource();
        await _tcs.Task;
    }
    // Al final de CustomSnackbar, antes del último }
    public async Task ShowAsync(string message, SnackbarType type)
    {
        Console.WriteLine($"[CustomSnackbar] ShowAsync llamado: msg={message}, type={type}");
        var (icon, color) = type.GetStyle();
        Console.WriteLine($"[CustomSnackbar] icon={icon}, color={color}");

        if (IsVisible)
            await Hide();

        await Show(message, icon, color);
    }

    public async Task Hide()
    {
        if (!IsVisible)
            return;

        // Animación salida (↓ y fade)
        await Task.WhenAll(
            this.TranslateToAsync(0, 80, 200, Easing.CubicIn),
            this.FadeToAsync(0, 150),
            this.ScaleToAsync(0.95, 150)
        );

        IsVisible = false;

        _tcs?.TrySetResult();
    }

}