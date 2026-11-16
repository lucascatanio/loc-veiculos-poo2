using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Locadora.Desktop.Conversores;

public class TextoParaVisibilidade : IValueConverter
{
    public bool Inverter { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        string.IsNullOrWhiteSpace(value as string) == Inverter ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
