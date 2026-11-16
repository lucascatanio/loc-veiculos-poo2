namespace Locadora.Domain.Consultas;

public class LocacaoVencendoItem
{
    public int LocacaoId { get; init; }
    public string ClienteNome { get; init; } = "";
    public string ClienteTelefone { get; init; } = "";
    public string VeiculoPlaca { get; init; } = "";
    public string VeiculoModelo { get; init; } = "";
    public DateTime DataPrevistaDevolucao { get; init; }
}
