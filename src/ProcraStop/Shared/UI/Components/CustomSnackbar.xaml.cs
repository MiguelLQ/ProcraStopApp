namespace ProcraStop.Shared.UI.Components;

using Microsoft.Maui.Controls;
using ProcraStop.Diagnostics;
using ProcraStop.Shared.Helpers;

public partial class CustomSnackbar : ContentView
{
    private const int AutoHideMilliseconds = 4000;

    // Serializa las peticiones para que dos snackbars no animen a la vez.
    private readonly SemaphoreSlim _gate = new(1, 1);

    private CancellationTokenSource? _autoHideCts;
    private int _generation;
    private bool _hiding;

    public CustomSnackbar()
    {
        InitializeComponent();

        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;
        GestureRecognizers.Add(tap);
    }

    private async void OnTapped(object? sender, TappedEventArgs e)
    {
        await SafeHideAsync("CustomSnackbar.Tapped");
    }

    /// <summary>
    /// Muestra el snackbar y vuelve tras la animación de entrada. El cierre es
    /// automático a los pocos segundos, de modo que quien espera <see cref="ShowAsync"/>
    /// no queda bloqueado hasta que el usuario toque el mensaje.
    /// </summary>
    public async Task Show(string message, string icon, Color color)
    {
        var generation = Interlocked.Increment(ref _generation);

        await _gate.WaitAsync();
        try
        {
            // HideNow (al salir de la página) incrementa la generación. Si esta
            // petición quedó en cola, ya no debe mostrarse sobre una página que
            // el usuario abandonó.
            if (generation != Volatile.Read(ref _generation))
                return;

            CancelAutoHide();

            Message.Text = message;
            Icon.Text = icon;
            IconContainer.BackgroundColor = color;

            IsVisible = true;
            TranslationY = 120;
            Scale = 0.9;
            Opacity = 0;

            await AnimateInAsync();

            if (generation == Volatile.Read(ref _generation))
            {
                _autoHideCts = new CancellationTokenSource();
                _ = AutoHideAsync(generation, _autoHideCts.Token);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task ShowAsync(string message, SnackbarType type)
    {
        try
        {
            var (icon, color) = type.GetStyle();

            if (IsVisible)
                await Hide();

            await Show(message, icon, color);
        }
        catch (Exception exception)
        {
            // Un snackbar nunca debe romper el flujo que lo invoca.
            AppCrashLogger.Log(exception, "CustomSnackbar.ShowAsync");
        }
    }

    public async Task Hide()
    {
        if (_hiding || !IsVisible)
            return;

        _hiding = true;
        try
        {
            CancelAutoHide();
            await AnimateOutAsync();
            IsVisible = false;
        }
        finally
        {
            _hiding = false;
        }
    }

    /// <summary>
    /// Oculta sin animar e invalida cualquier cierre automático pendiente. Se
    /// usa al salir de la página para no dejar animaciones sobre un handler
    /// que ya fue desconectado.
    /// </summary>
    public void HideNow()
    {
        Interlocked.Increment(ref _generation);
        CancelAutoHide();
        IsVisible = false;
        TranslationY = 0;
        Scale = 1;
        Opacity = 0;
    }

    private async Task AutoHideAsync(int generation, CancellationToken token)
    {
        try
        {
            await Task.Delay(AutoHideMilliseconds, token);

            if (generation != Volatile.Read(ref _generation))
                return;

            await Hide();
        }
        catch (OperationCanceledException)
        {
            // Hide ya fue llamado manualmente o la página se cerró.
        }
        catch (Exception exception)
        {
            AppCrashLogger.Log(exception, "CustomSnackbar.AutoHide");
        }
    }

    private async Task SafeHideAsync(string context)
    {
        try
        {
            await Hide();
        }
        catch (Exception exception)
        {
            AppCrashLogger.Log(exception, context);
        }
    }

    private async Task AnimateInAsync()
    {
        if (Handler is null)
        {
            TranslationY = 0;
            Scale = 1;
            Opacity = 1;
            return;
        }

        await Task.WhenAll(
            this.TranslateToAsync(0, 0, 300, Easing.CubicOut),
            this.FadeToAsync(1, 250),
            this.ScaleToAsync(1, 250, Easing.CubicOut));

        await this.ScaleToAsync(1.03, 100);
        await this.ScaleToAsync(1, 80);
    }

    private async Task AnimateOutAsync()
    {
        if (Handler is null)
        {
            TranslationY = 0;
            Scale = 1;
            Opacity = 0;
            return;
        }

        await Task.WhenAll(
            this.TranslateToAsync(0, 80, 200, Easing.CubicIn),
            this.FadeToAsync(0, 150),
            this.ScaleToAsync(0.95, 150));
    }

    private void CancelAutoHide()
    {
        var cts = _autoHideCts;
        _autoHideCts = null;

        if (cts is null)
            return;

        try
        {
            cts.Cancel();
            cts.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Ya estaba liberado; no hay nada que hacer.
        }
    }
}
