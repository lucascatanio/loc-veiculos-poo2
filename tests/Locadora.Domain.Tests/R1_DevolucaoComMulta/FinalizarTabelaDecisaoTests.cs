using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Tests.Apoio;

namespace Locadora.Domain.Tests.R1_DevolucaoComMulta;

// Condições de Locacao.Finalizar:
//   C1 locação aberta | C2 km de devolução >= km de retirada | C3 só pagamentos de saída
//   C4 saídas fecham o saldo | C5 devolução não é anterior à retirada
//
//          R1  R2  R3  R4  R5  R6  R7
//   C1     N   S   S   S   S   S   S
//   C2     -   N   S   S   S   S   S
//   C3     -   -   N   S   S   S   S
//   C4     -   -   -   N   N   S   S
//   C5     -   -   -   -   -   N   S
//   Ação   E   E   E   E   E   E   Finaliza       (E = DomainException)
[Trait(Tecnica.Requisito, Tecnica.Devolucao)]
[Trait(Tecnica.Nome, Tecnica.TabelaDecisao)]
public class FinalizarTabelaDecisaoTests
{
    private const int KmValido = Cenario.KmInicial + 500;

    [Fact]
    public void R1_LocacaoFinalizada_LancaExcecao()
    {
        var locacao = Cenario.LocacaoFinalizada();

        Assert.Throws<DomainException>(() =>
            locacao.Finalizar(Cenario.Prevista, KmValido, null, [Cenario.Saida(100m)]));
    }

    [Fact]
    public void R1_LocacaoCancelada_LancaExcecao()
    {
        var locacao = Cenario.LocacaoCancelada();

        Assert.Throws<DomainException>(() =>
            locacao.Finalizar(Cenario.Prevista, KmValido, null, [Cenario.Saida(Cenario.PrevistoTresDiarias)]));
    }

    [Fact]
    public void R2_KmMenorQueRetirada_LancaExcecao()
    {
        var locacao = Cenario.LocacaoAberta();

        Assert.Throws<DomainException>(() =>
            locacao.Finalizar(Cenario.Prevista, Cenario.KmInicial - 1, null, [Cenario.Saida(Cenario.PrevistoTresDiarias)]));
    }

    [Fact]
    public void R3_PagamentoDoTipoEntrada_LancaExcecao()
    {
        var locacao = Cenario.LocacaoAberta();

        Assert.Throws<DomainException>(() =>
            locacao.Finalizar(Cenario.Prevista, KmValido, null, [Cenario.Entrada(Cenario.PrevistoTresDiarias)]));
    }

    [Fact]
    public void R4_SaidasNaoCobremSaldo_LancaExcecaoInformandoFalta()
    {
        var locacao = Cenario.LocacaoAberta();

        var erro = Assert.Throws<DomainException>(() =>
            locacao.Finalizar(Cenario.Prevista, KmValido, null, [Cenario.Saida(200m)]));

        Assert.Contains("Falta R$ 100,00", erro.Message);
    }

    [Fact]
    public void R5_SaidasPassamDoSaldo_LancaExcecaoInformandoSobra()
    {
        var locacao = Cenario.LocacaoAberta();

        var erro = Assert.Throws<DomainException>(() =>
            locacao.Finalizar(Cenario.Prevista, KmValido, null, [Cenario.Saida(350m)]));

        Assert.Contains("Sobra R$ 50,00", erro.Message);
    }

    [Fact]
    public void R6_DevolucaoAntesDaRetirada_LancaExcecao()
    {
        var locacao = Cenario.LocacaoAberta();

        Assert.Throws<DomainException>(() =>
            locacao.Finalizar(Cenario.Retirada.AddHours(-1), KmValido, null, [Cenario.Saida(Cenario.PrevistoTresDiarias)]));
    }

    [Fact]
    public void R7_TodasAsCondicoesAtendidas_FinalizaCobrandoMulta()
    {
        var locacao = Cenario.LocacaoAberta();
        locacao.RegistrarPagamento(Cenario.Entrada(100m));
        var devolucao = Cenario.Prevista.AddHours(26);

        locacao.Finalizar(devolucao, KmValido, "  riscado na porta  ", [Cenario.Saida(440m)]);

        Assert.Equal(StatusLocacao.Finalizada, locacao.Status);
        Assert.Equal(240m, locacao.ValorMulta);
        Assert.Equal(540m, locacao.ValorTotal);
        Assert.Equal(540m, locacao.ValorPago);
        Assert.Equal(0m, locacao.SaldoDevedor());
        Assert.Equal(devolucao, locacao.DataDevolucao);
        Assert.Equal(KmValido, locacao.KmDevolucao);
        Assert.Equal("riscado na porta", locacao.ObservacaoDevolucao);
        Assert.Equal(2, locacao.Pagamentos.Count);
        Assert.Equal(StatusVeiculo.Disponivel, locacao.Veiculo.Status);
        Assert.Equal(KmValido, locacao.Veiculo.KmAtual);
    }

    [Fact]
    public void R7_KmIgualAoDaRetirada_Finaliza()
    {
        var locacao = Cenario.LocacaoAberta();

        locacao.Finalizar(Cenario.Prevista, Cenario.KmInicial, null, [Cenario.Saida(Cenario.PrevistoTresDiarias)]);

        Assert.Equal(StatusLocacao.Finalizada, locacao.Status);
    }

    [Fact]
    public void R7_SaldoQuitadoNaAberturaESemSaidas_Finaliza()
    {
        var locacao = Cenario.LocacaoAberta();
        locacao.RegistrarPagamento(Cenario.Entrada(Cenario.PrevistoTresDiarias));

        locacao.Finalizar(Cenario.Prevista, KmValido, null, []);

        Assert.Equal(StatusLocacao.Finalizada, locacao.Status);
        Assert.Equal(Cenario.PrevistoTresDiarias, locacao.ValorPago);
    }

    [Fact]
    public void RegraVioladaNaoAlteraLocacaoNemVeiculo()
    {
        var locacao = Cenario.LocacaoAberta();

        Assert.Throws<DomainException>(() =>
            locacao.Finalizar(Cenario.Prevista, KmValido, null, [Cenario.Saida(200m)]));

        Assert.Equal(StatusLocacao.Aberta, locacao.Status);
        Assert.Empty(locacao.Pagamentos);
        Assert.Null(locacao.DataDevolucao);
        Assert.Equal(StatusVeiculo.Locado, locacao.Veiculo.Status);
        Assert.Equal(Cenario.KmInicial, locacao.Veiculo.KmAtual);
    }
}
