using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Locadora.Desktop.Conversores;

public class StatusParaBrush : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var chave = (value as string)?.ToLowerInvariant() switch
        {
            "disponivel" => "LiberadoBrush",
            "aberta" or "locado" => "AzulMercosulBrush",
            "manutencao" => "FaixaAmarelaBrush",
            "atrasada" or "erro" => "SinalVermelhoBrush",
            _ => "TextoSecundarioBrush"
        };
        return Application.Current.FindResource(chave);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
