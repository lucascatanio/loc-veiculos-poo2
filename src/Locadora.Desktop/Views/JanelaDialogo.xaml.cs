using System.Windows;
using System.Windows.Input;

namespace Locadora.Desktop.Views;

public partial class JanelaDialogo : Window
{
    public JanelaDialogo(string titulo, string mensagem, string textoConfirmar, string? textoCancelar)
    {
        InitializeComponent();
        Titulo.Text = titulo;
        Mensagem.Text = mensagem;
        Confirmar.Content = textoConfirmar;
        Cancelar.Content = textoCancelar;
        Cancelar.Visibility = textoCancelar is null ? Visibility.Collapsed : Visibility.Visible;
    }

    private void Confirmar_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    private void Cancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Arrastar(object sender, MouseButtonEventArgs e) => DragMove();
}
