using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Locadora.Desktop.Controles;

public enum TomValor
{
    Normal,
    Alerta,
    Erro,
    Sucesso
}

public partial class Valor : UserControl
{
    public static readonly DependencyProperty QuantiaProperty = DependencyProperty.Register(
        nameof(Quantia), typeof(decimal), typeof(Valor), new PropertyMetadata(0m));

    public static readonly DependencyProperty RotuloProperty = DependencyProperty.Register(
        nameof(Rotulo), typeof(string), typeof(Valor), new PropertyMetadata(null));

    public static readonly DependencyProperty DestaqueProperty = DependencyProperty.Register(
        nameof(Destaque), typeof(bool), typeof(Valor),
        new PropertyMetadata(false, (d, _) => ((Valor)d).Aplicar()));

    public static readonly DependencyProperty TomProperty = DependencyProperty.Register(
        nameof(Tom), typeof(TomValor), typeof(Valor),
        new PropertyMetadata(TomValor.Normal, (d, _) => ((Valor)d).Aplicar()));

    public decimal Quantia
    {
        get => (decimal)GetValue(QuantiaProperty);
        set => SetValue(QuantiaProperty, value);
    }

    public string? Rotulo
    {
        get => (string?)GetValue(RotuloProperty);
        set => SetValue(RotuloProperty, value);
    }

    public bool Destaque
    {
        get => (bool)GetValue(DestaqueProperty);
        set => SetValue(DestaqueProperty, value);
    }

    public TomValor Tom
    {
        get => (TomValor)GetValue(TomProperty);
        set => SetValue(TomProperty, value);
    }

    public Valor()
    {
        InitializeComponent();
        Aplicar();
    }

    private void Aplicar()
    {
        Numero.FontSize = Destaque ? 28 : 16;
        Numero.FontFamily = (FontFamily)FindResource(Destaque ? "BarlowCondensedSemiBold" : "BarlowCondensedMedium");
        Numero.Foreground = (Brush)FindResource(Tom switch
        {
            TomValor.Alerta => "AtencaoTextoBrush",
            TomValor.Erro => "SinalVermelhoBrush",
            TomValor.Sucesso => "LiberadoBrush",
            _ => "AsfaltoBrush"
        });
    }
}
