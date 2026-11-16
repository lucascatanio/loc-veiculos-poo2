using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Locadora.Desktop.Controles;

public partial class BarraBusca : UserControl
{
    public static readonly DependencyProperty TextoBuscaProperty = DependencyProperty.Register(
        nameof(TextoBusca), typeof(string), typeof(BarraBusca),
        new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (d, _) => ((BarraBusca)d).Buscar()));

    public static readonly DependencyProperty MostrarInativosProperty = DependencyProperty.Register(
        nameof(MostrarInativos), typeof(bool), typeof(BarraBusca),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (d, _) => ((BarraBusca)d).Buscar()));

    public static readonly DependencyProperty ComandoBuscarProperty = DependencyProperty.Register(
        nameof(ComandoBuscar), typeof(ICommand), typeof(BarraBusca), new PropertyMetadata(null));

    public static readonly DependencyProperty DicaProperty = DependencyProperty.Register(
        nameof(Dica), typeof(string), typeof(BarraBusca), new PropertyMetadata("Buscar por nome"));

    public string TextoBusca
    {
        get => (string)GetValue(TextoBuscaProperty);
        set => SetValue(TextoBuscaProperty, value);
    }

    public bool MostrarInativos
    {
        get => (bool)GetValue(MostrarInativosProperty);
        set => SetValue(MostrarInativosProperty, value);
    }

    public ICommand? ComandoBuscar
    {
        get => (ICommand?)GetValue(ComandoBuscarProperty);
        set => SetValue(ComandoBuscarProperty, value);
    }

    public string Dica
    {
        get => (string)GetValue(DicaProperty);
        set => SetValue(DicaProperty, value);
    }

    public BarraBusca() => InitializeComponent();

    private void Buscar()
    {
        if (ComandoBuscar?.CanExecute(null) == true)
            ComandoBuscar.Execute(null);
    }
}
