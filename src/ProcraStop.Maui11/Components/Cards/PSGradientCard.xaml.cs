using System.Windows.Input;

namespace ProcraStop.Maui11.Components.Cards;

/// <summary>
/// Card hero con degradado del Primary (estilo Grial). View-only.
/// Ejemplo:
/// <code>
/// &lt;cards:PSGradientCard Glyph="{x:Static fonts:FluentUI.timer_24_regular}"
///                        Title="Sesión de foco"
///                        Subtitle="25 min · Pomodoro"
///                        Command="{Binding StartFocusCommand}" /&gt;
/// </code>
/// El degradado cambia con el tema (PrimaryGradientLight/Dark).
/// </summary>
public partial class PSGradientCard : Border
{
    public static readonly BindableProperty GlyphProperty =
        BindableProperty.Create(nameof(Glyph), typeof(string), typeof(PSGradientCard), default(string));

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PSGradientCard), default(string));

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(PSGradientCard), default(string));

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(PSGradientCard), default(ICommand));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(PSGradientCard), default(object));

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public PSGradientCard() => InitializeComponent();
}
