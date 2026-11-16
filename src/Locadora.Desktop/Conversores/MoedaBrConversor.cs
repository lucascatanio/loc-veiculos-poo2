using System.Globalization;
using System.Windows.Data;

namespace Locadora.Desktop.Conversores;

public class MoedaBrConversor : IValueConverter
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is decimal valor ? valor.ToString("C", PtBr) : "";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        decimal.TryParse(value as string, NumberStyles.Currency, PtBr, out var valor) ? valor : Binding.DoNothing;
}
