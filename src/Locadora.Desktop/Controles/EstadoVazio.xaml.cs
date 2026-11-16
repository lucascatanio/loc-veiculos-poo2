using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Locadora.Desktop.Controles;

public partial class EstadoVazio : UserControl
{
    public static readonly DependencyProperty TextoProperty = DependencyProperty.Register(
        nameof(Texto), typeof(string), typeof(EstadoVazio), new PropertyMetadata(null));

    public static readonly DependencyProperty TextoAcaoProperty = DependencyProperty.Register(
        nameof(TextoAcao), typeof(string), typeof(EstadoVazio), new PropertyMetadata(null));

    public static readonly DependencyProperty ComandoProperty = DependencyProperty.Register(
        nameof(Comando), typeof(ICommand), typeof(EstadoVazio), new PropertyMetadata(null));

    public string? Texto
    {
        get => (string?)GetValue(TextoProperty);
        set => SetValue(TextoProperty, value);
    }

    public string? TextoAcao
    {
        get => (string?)GetValue(TextoAcaoProperty);
        set => SetValue(TextoAcaoProperty, value);
    }

    public ICommand? Comando
    {
        get => (ICommand?)GetValue(ComandoProperty);
        set => SetValue(ComandoProperty, value);
    }

    public EstadoVazio() => InitializeComponent();
}
