using System.Windows;
using System.Windows.Controls;

namespace Locadora.Desktop.Controles;

public partial class MensagemErro : UserControl
{
    public static readonly DependencyProperty MensagemProperty = DependencyProperty.Register(
        nameof(Mensagem), typeof(string), typeof(MensagemErro), new PropertyMetadata(null));

    public string? Mensagem
    {
        get => (string?)GetValue(MensagemProperty);
        set => SetValue(MensagemProperty, value);
    }

    public MensagemErro() => InitializeComponent();
}
