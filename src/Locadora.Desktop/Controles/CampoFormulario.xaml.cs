using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Locadora.Desktop.Controles;

[ContentProperty(nameof(Conteudo))]
public partial class CampoFormulario : UserControl
{
    public static readonly DependencyProperty RotuloProperty = DependencyProperty.Register(
        nameof(Rotulo), typeof(string), typeof(CampoFormulario), new PropertyMetadata(null));

    public static readonly DependencyProperty ConteudoProperty = DependencyProperty.Register(
        nameof(Conteudo), typeof(object), typeof(CampoFormulario), new PropertyMetadata(null));

    public static readonly DependencyProperty AjudaProperty = DependencyProperty.Register(
        nameof(Ajuda), typeof(string), typeof(CampoFormulario), new PropertyMetadata(null));

    public static readonly DependencyProperty ErroProperty = DependencyProperty.Register(
        nameof(Erro), typeof(string), typeof(CampoFormulario), new PropertyMetadata(null));

    public string? Rotulo
    {
        get => (string?)GetValue(RotuloProperty);
        set => SetValue(RotuloProperty, value);
    }

    public object? Conteudo
    {
        get => GetValue(ConteudoProperty);
        set => SetValue(ConteudoProperty, value);
    }

    public string? Ajuda
    {
        get => (string?)GetValue(AjudaProperty);
        set => SetValue(AjudaProperty, value);
    }

    public string? Erro
    {
        get => (string?)GetValue(ErroProperty);
        set => SetValue(ErroProperty, value);
    }

    public CampoFormulario() => InitializeComponent();
}
