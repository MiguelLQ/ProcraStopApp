using System.Windows.Input;

namespace ProcraStop.Maui11.Components.Common;

/// <summary>
/// Chip de filtro/acción compacto (radius 14). El estado Selected pinta
/// PrimaryContainer + stroke Primary (trigger interno).
/// Ejemplo:
/// <code>
/// &lt;common:PSChip Text="Hoy" IsSelected="{Binding IsSelected}"
///                Command="{Binding ToggleFilterCommand}" /&gt;
/// </code>
/// </summary>
public partial class PSChip : Border
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(PSChip), default(string));

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(PSChip), false);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(PSChip), default(ICommand));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(PSChip), default(object));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
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

    public PSChip() => InitializeComponent();
}
