using System.Globalization;

namespace Locadora.Desktop.ViewModels.Locacoes;

public static class ValorMonetario
{
    private const NumberStyles EstiloDoValor = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static bool TentarLer(string? texto, out decimal valor) =>
        decimal.TryParse(texto, EstiloDoValor, PtBr, out valor);

    public static string Formatar(decimal valor) => valor.ToString("C", PtBr);
}
