namespace ProcraStop.Maui11.Components.Common;

/// <summary>
/// Badge pequeño no interactivo (contador/estado).
/// Ejemplo:
/// <code>
/// &lt;common:PSBadge Text="3 nuevas" Style="{StaticResource PSBadgeSuccessStyle}" /&gt;
/// </code>
/// Variantes de color: PSBadgePrimaryStyle / Success / Warning / Error.
/// </summary>
public partial class PSBadge : Border
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(PSBadge), default(string));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public PSBadge() => InitializeComponent();
}
