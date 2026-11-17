using System.Globalization;
using System.Windows.Data;

namespace Locadora.Desktop.Conversores;

public class QuilometragemConversor : IValueConverter
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is int quilometros ? $"{quilometros.ToString("N0", PtBr)} km" : "";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
