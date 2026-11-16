namespace Locadora.Domain.Consultas;

public class LocacaoAtrasadaItem
{
    public int LocacaoId { get; init; }
    public string ClienteNome { get; init; } = "";
    public string ClienteTelefone { get; init; } = "";
    public string VeiculoPlaca { get; init; } = "";
    public string VeiculoModelo { get; init; } = "";
    public DateTime DataPrevistaDevolucao { get; init; }
    public int MinutosAtraso { get; init; }
    public int DiasAtraso { get; init; }
    public decimal MultaEstimada { get; init; }
}
