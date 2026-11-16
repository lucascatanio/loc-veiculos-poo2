using System.Windows;
using System.Windows.Controls;

namespace Locadora.Desktop.Views;

public partial class LoginView : UserControl
{
    public LoginView() => InitializeComponent();

    private void AoCarregar(object sender, RoutedEventArgs e) => CampoEmail.Focus();
}
