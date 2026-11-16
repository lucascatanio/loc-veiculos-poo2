using System.Globalization;
using System.Windows.Data;

namespace Locadora.Desktop.Conversores;

public class TelefoneConversor : IValueConverter
{
    private const int DigitosComCelular = 11;
    private const int DigitosFixo = 10;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var texto = value as string ?? "";
        var digitos = new string(texto.Where(char.IsDigit).ToArray());
        return digitos.Length switch
        {
            DigitosComCelular => $"({digitos[..2]}) {digitos[2..7]}-{digitos[7..]}",
            DigitosFixo => $"({digitos[..2]}) {digitos[2..6]}-{digitos[6..]}",
            _ => texto
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
