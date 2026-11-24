using System.Globalization;
using System.Windows.Data;

namespace Locadora.Desktop.Conversores;

public class MinutosParaAtrasoConversor : IValueConverter
{
    private const int MinutosPorHora = 60;
    private const int MinutosPorDia = 24 * MinutosPorHora;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int minutos) return "";
        if (minutos < MinutosPorHora) return $"{minutos} min";
        if (minutos < MinutosPorDia) return $"{minutos / MinutosPorHora} h";

        var dias = (int)Math.Ceiling((double)minutos / MinutosPorDia);
        return dias == 1 ? "1 dia" : $"{dias} dias";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
