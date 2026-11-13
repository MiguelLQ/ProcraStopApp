using System.Windows.Input;

namespace ProcraStop.Maui11.Components.Buttons;

/// <summary>
/// FAB circular primario (56×56) con glyph "+" de FluentUI. Delegación
/// trivial al Command del consumidor (MVVM: la lógica vive en el ViewModel).
/// Ejemplo:
/// <code>&lt;components:PSFab Command="{Binding OpenAddSheetCommand}" /&gt;</code>
/// Accesibilidad: fijar SemanticProperties.Description al usarlo.
/// </summary>
public partial class PSFab : Border
{
    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(PSFab));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(PSFab));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public PSFab() => InitializeComponent();

    private void OnFabTapped(object? sender, EventArgs e)
    {
        if (Command?.CanExecute(CommandParameter) == true)
            Command.Execute(CommandParameter);
    }
}
