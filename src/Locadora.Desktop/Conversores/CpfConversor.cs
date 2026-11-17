using System.Globalization;
using System.Windows.Data;

namespace Locadora.Desktop.Conversores;

public class CpfConversor : IValueConverter
{
    private const int DigitosDoCpf = 11;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var texto = value as string ?? "";
        var digitos = new string(texto.Where(char.IsDigit).ToArray());
        return digitos.Length == DigitosDoCpf
            ? $"{digitos[..3]}.{digitos[3..6]}.{digitos[6..9]}-{digitos[9..]}"
            : texto;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
