using Locadora.Api.Dtos.Locacoes;

namespace Locadora.Desktop.ViewModels.Locacoes;

public enum SituacaoDaPrevista
{
    NoPrazo,
    Hoje,
    Atrasada
}

public sealed class LocacaoLinha(LocacaoResumoResponse resumo, DateTime agora)
{
    public int Id => resumo.Id;
    public string ClienteNome => resumo.ClienteNome;
    public string VeiculoPlaca => resumo.VeiculoPlaca;
    public DateTime DataRetirada => resumo.DataRetirada;
    public DateTime DataPrevistaDevolucao => resumo.DataPrevistaDevolucao;
    public decimal ValorTotal => resumo.ValorTotal;
    public decimal SaldoDevedor => resumo.SaldoDevedor;

    public SituacaoDaPrevista Situacao => CalcularSituacao();

    private SituacaoDaPrevista CalcularSituacao()
    {
        if (resumo.Status != OpcaoDeStatus.Abertas.Valor) return SituacaoDaPrevista.NoPrazo;
        if (DataPrevistaDevolucao < agora) return SituacaoDaPrevista.Atrasada;
        return DataPrevistaDevolucao.Date == agora.Date ? SituacaoDaPrevista.Hoje : SituacaoDaPrevista.NoPrazo;
    }
}
