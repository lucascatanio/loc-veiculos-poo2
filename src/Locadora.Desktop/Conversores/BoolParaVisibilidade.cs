using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Locadora.Desktop.Conversores;

public class BoolParaVisibilidade : IValueConverter
{
    public bool Inverter { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (value is true) != Inverter ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        (value is Visibility.Visible) != Inverter;
}
