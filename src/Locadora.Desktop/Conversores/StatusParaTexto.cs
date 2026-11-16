using System.Globalization;
using System.Windows.Data;

namespace Locadora.Desktop.Conversores;

public class StatusParaTexto : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (value as string)?.ToLowerInvariant() switch
        {
            "disponivel" => "Disponível",
            "manutencao" => "Manutenção",
            "locado" => "Locado",
            "aberta" => "Aberta",
            "finalizada" => "Finalizada",
            "cancelada" => "Cancelada",
            "atrasada" => "Atrasada",
            "inativo" => "Inativo",
            _ => value as string ?? ""
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
