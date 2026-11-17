using System.Globalization;

namespace Locadora.Desktop.ViewModels.Locacoes;

// Pré-visualização do contrato enquanto o formulário é preenchido: quem confirma valores e diárias é a API.
public sealed record ResumoDaRetirada(
    int DiariasPrevistas,
    decimal TotalPrevisto,
    decimal TotalEntradas,
    IReadOnlyList<FatoDoContrato> Fatos,
    IReadOnlyList<TotalDoContrato> Totais,
    string? AvisoDeData,
    string? AvisoDeEntradas)
{
    private const int HorasPorDiaria = 24;
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static ResumoDaRetirada Inicial { get; } = Prever(null, null, null, 0, DateTime.Now);

    public decimal Saldo => TotalPrevisto - TotalEntradas;
    public bool PossuiValores => Totais.Count > 0;

    public static int PreverDiarias(DateTime retirada, DateTime devolucaoPrevista) =>
        Math.Max(1, (int)Math.Ceiling((devolucaoPrevista - retirada).TotalHours / HorasPorDiaria));

    public static ResumoDaRetirada Prever(
        string? clienteNome, VeiculoParaRetirada? veiculo, DateTime? devolucaoPrevista, decimal totalEntradas, DateTime agora)
    {
        var fatos = MontarFatos(clienteNome, devolucaoPrevista);
        var avisoDeData = devolucaoPrevista < agora ? "A devolução prevista já passou." : null;

        if (veiculo is null || devolucaoPrevista is null)
            return new(0, 0, totalEntradas, fatos, [], avisoDeData, null);

        var diarias = PreverDiarias(agora, devolucaoPrevista.Value);
        var totalPrevisto = Math.Round(diarias * veiculo.ValorDiaria, 2, MidpointRounding.AwayFromZero);
        var avisoDeEntradas = totalEntradas > totalPrevisto ? "As entradas somam mais que o total previsto." : null;
        var totais = MontarTotais(veiculo.ValorDiaria, diarias, totalPrevisto, totalEntradas);
        return new(diarias, totalPrevisto, totalEntradas, fatos, totais, avisoDeData, avisoDeEntradas);
    }

    private static List<FatoDoContrato> MontarFatos(string? clienteNome, DateTime? devolucaoPrevista)
    {
        var fatos = new List<FatoDoContrato>();
        if (clienteNome is not null)
            fatos.Add(new("Cliente", clienteNome));
        if (devolucaoPrevista is { } devolucao)
            fatos.Add(new("Devolução prevista", devolucao.ToString("dd/MM/yyyy HH:mm", PtBr)));
        return fatos;
    }

    private static List<TotalDoContrato> MontarTotais(decimal valorDiaria, int diarias, decimal totalPrevisto, decimal totalEntradas) =>
    [
        new("Diária da categoria", valorDiaria),
        new($"Previsto ({diarias} {(diarias == 1 ? "diária" : "diárias")})", totalPrevisto),
        new("Entradas", totalEntradas)
    ];
}
