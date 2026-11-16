using System.Globalization;
using System.Windows.Data;

namespace Locadora.Desktop.Conversores;

public class ParticipacaoParaLarguraConversor : IValueConverter
{
    public double LarguraTotal { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is double participacao ? participacao * LarguraTotal : 0.0;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
