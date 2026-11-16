using System.Windows;
using System.Windows.Controls;

namespace Locadora.Desktop.Controles;

public enum TamanhoPlaca
{
    Pequeno,
    Grande
}

public partial class PlacaMercosul : UserControl
{
    private const double LARGURA_PEQUENA = 120;
    private const double LARGURA_GRANDE = 240;
    private const double PROPORCAO = 240.0 / 78.0;

    public static readonly DependencyProperty PlacaProperty = DependencyProperty.Register(
        nameof(Placa), typeof(string), typeof(PlacaMercosul),
        new PropertyMetadata(null, (d, _) => ((PlacaMercosul)d).Atualizar()));

    public static readonly DependencyProperty TamanhoProperty = DependencyProperty.Register(
        nameof(Tamanho), typeof(TamanhoPlaca), typeof(PlacaMercosul),
        new PropertyMetadata(TamanhoPlaca.Pequeno, (d, _) => ((PlacaMercosul)d).Atualizar()));

    public string? Placa
    {
        get => (string?)GetValue(PlacaProperty);
        set => SetValue(PlacaProperty, value);
    }

    public TamanhoPlaca Tamanho
    {
        get => (TamanhoPlaca)GetValue(TamanhoProperty);
        set => SetValue(TamanhoProperty, value);
    }

    public PlacaMercosul()
    {
        InitializeComponent();
        Atualizar();
    }

    private void Atualizar()
    {
        var largura = Tamanho == TamanhoPlaca.Grande ? LARGURA_GRANDE : LARGURA_PEQUENA;
        Escala.Width = largura;
        Escala.Height = largura / PROPORCAO;
        Caracteres.ItemsSource = Separar(Normalizar(Placa));
    }

    private static string Normalizar(string? placa) =>
        new((placa ?? "").Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    // Formato antigo (AAA9999) leva hífen; Mercosul (AAA9A99) não.
    private static List<string> Separar(string placa)
    {
        var antiga = placa.Length == 7 && placa[4..].All(char.IsDigit);
        var caracteres = placa.Select(c => c.ToString()).ToList();
        if (antiga)
            caracteres.Insert(3, "-");
        return caracteres;
    }
}
