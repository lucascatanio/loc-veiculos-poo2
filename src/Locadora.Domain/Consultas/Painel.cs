namespace Locadora.Domain.Consultas;

public class Painel
{
    public List<LocacaoAtrasadaItem> Atrasadas { get; init; } = [];
    public List<LocacaoVencendoItem> VencendoHoje { get; init; } = [];
    public List<ContadorFrotaItem> Frota { get; init; } = [];
}
