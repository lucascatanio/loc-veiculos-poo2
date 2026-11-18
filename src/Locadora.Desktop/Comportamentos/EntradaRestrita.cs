using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Locadora.Desktop.Comportamentos;

public enum ModoDeEntrada
{
    Livre,
    Digitos,
    Decimal,
    Placa
}

// Impede de digitar ou colar o que o campo não aceita, junto com o MaxLength do TextBox.
// É só ajuda de preenchimento: quem decide o que é válido continua sendo a API.
public static partial class EntradaRestrita
{
    public static readonly DependencyProperty ModoProperty = DependencyProperty.RegisterAttached(
        "Modo", typeof(ModoDeEntrada), typeof(EntradaRestrita),
        new PropertyMetadata(ModoDeEntrada.Livre, AoMudarModo));

    public static ModoDeEntrada GetModo(DependencyObject o) => (ModoDeEntrada)o.GetValue(ModoProperty);
    public static void SetModo(DependencyObject o, ModoDeEntrada valor) => o.SetValue(ModoProperty, valor);

    private static void AoMudarModo(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        if (o is not TextBox caixa) return;

        caixa.PreviewTextInput -= AoDigitar;
        caixa.PreviewKeyDown -= AoPressionarTecla;
        DataObject.RemovePastingHandler(caixa, AoColar);

        if ((ModoDeEntrada)e.NewValue == ModoDeEntrada.Livre) return;

        caixa.PreviewTextInput += AoDigitar;
        caixa.PreviewKeyDown += AoPressionarTecla;
        DataObject.AddPastingHandler(caixa, AoColar);
    }

    private static void AoDigitar(object sender, TextCompositionEventArgs e)
    {
        var caixa = (TextBox)sender;
        e.Handled = !Aceita(GetModo(caixa), TextoApos(caixa, e.Text));
    }

    // A barra de espaço não gera PreviewTextInput no TextBox.
    private static void AoPressionarTecla(object sender, KeyEventArgs e)
    {
        var caixa = (TextBox)sender;
        if (e.Key == Key.Space && !Aceita(GetModo(caixa), TextoApos(caixa, " ")))
            e.Handled = true;
    }

    private static void AoColar(object sender, DataObjectPastingEventArgs e)
    {
        e.CancelCommand();
        if (e.DataObject.GetData(DataFormats.UnicodeText) is not string colado) return;

        var caixa = (TextBox)sender;
        var modo = GetModo(caixa);
        var aproveitado = new string(colado.Where(c => CaractereValido(modo, c)).ToArray());

        if (caixa.MaxLength > 0)
        {
            var espaco = caixa.MaxLength - (caixa.Text.Length - caixa.SelectionLength);
            aproveitado = aproveitado[..Math.Clamp(espaco, 0, aproveitado.Length)];
        }

        if (aproveitado.Length > 0 && Aceita(modo, TextoApos(caixa, aproveitado)))
            caixa.SelectedText = aproveitado;
    }

    private static string TextoApos(TextBox caixa, string inserido) =>
        caixa.Text.Remove(caixa.SelectionStart, caixa.SelectionLength).Insert(caixa.SelectionStart, inserido);

    private static bool Aceita(ModoDeEntrada modo, string texto) => modo switch
    {
        ModoDeEntrada.Digitos => texto.All(char.IsAsciiDigit),
        ModoDeEntrada.Placa => texto.All(char.IsAsciiLetterOrDigit),
        ModoDeEntrada.Decimal => ValorComVirgula().IsMatch(texto),
        _ => true
    };

    private static bool CaractereValido(ModoDeEntrada modo, char c) => modo switch
    {
        ModoDeEntrada.Digitos => char.IsAsciiDigit(c),
        ModoDeEntrada.Placa => char.IsAsciiLetterOrDigit(c),
        ModoDeEntrada.Decimal => char.IsAsciiDigit(c) || c == ',',
        _ => true
    };

    // Só dígitos, com no máximo uma vírgula e duas casas depois dela (centavos).
    [GeneratedRegex(@"^\d*(,\d{0,2})?$")]
    private static partial Regex ValorComVirgula();
}
