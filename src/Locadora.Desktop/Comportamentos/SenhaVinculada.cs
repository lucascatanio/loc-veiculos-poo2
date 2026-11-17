using System.Windows;
using System.Windows.Controls;

namespace Locadora.Desktop.Comportamentos;

// PasswordBox.Password não é uma DependencyProperty; este attached behavior a expõe para binding.
// O padrão é null (e não ""): o binding inicial entrega "" e só uma mudança de valor liga o PasswordChanged,
// senão o que se digita nunca chega ao ViewModel.
public static class SenhaVinculada
{
    public static readonly DependencyProperty SenhaProperty = DependencyProperty.RegisterAttached(
        "Senha", typeof(string), typeof(SenhaVinculada),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AoMudarSenha));

    private static readonly DependencyProperty AtualizandoProperty =
        DependencyProperty.RegisterAttached("Atualizando", typeof(bool), typeof(SenhaVinculada));

    public static string GetSenha(DependencyObject o) => (string)o.GetValue(SenhaProperty);
    public static void SetSenha(DependencyObject o, string valor) => o.SetValue(SenhaProperty, valor);

    private static void AoMudarSenha(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        if (o is not PasswordBox caixa) return;

        caixa.PasswordChanged -= AoDigitar;
        if (!(bool)caixa.GetValue(AtualizandoProperty) && caixa.Password != (string)e.NewValue)
            caixa.Password = (string?)e.NewValue ?? "";
        caixa.PasswordChanged += AoDigitar;
    }

    private static void AoDigitar(object sender, RoutedEventArgs e)
    {
        var caixa = (PasswordBox)sender;
        caixa.SetValue(AtualizandoProperty, true);
        SetSenha(caixa, caixa.Password);
        caixa.SetValue(AtualizandoProperty, false);
    }
}
