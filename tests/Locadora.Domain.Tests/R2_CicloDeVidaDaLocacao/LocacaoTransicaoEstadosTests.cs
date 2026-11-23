using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Tests.Apoio;

namespace Locadora.Domain.Tests.R2_CicloDeVidaDaLocacao;

//                      Aberta        Finalizada   Cancelada
//   RegistrarPagamento Aberta        E            E
//   Prorrogar          Aberta        E            E
//   CalcularPrevia     Aberta        E            E
//   Finalizar          Finalizada    E            E
//   Cancelar           Cancelada     E            E          (E = DomainException, estado mantido)
[Trait(Tecnica.Requisito, Tecnica.CicloLocacao)]
[Trait(Tecnica.Nome, Tecnica.TransicaoEstados)]
public class LocacaoTransicaoEstadosTests
{
    private static readonly Dictionary<StatusLocacao, Func<Locacao>> Estados = new()
    {
        [StatusLocacao.Aberta] = () => Cenario.LocacaoAberta(),
        [StatusLocacao.Finalizada] = Cenario.LocacaoFinalizada,
        [StatusLocacao.Cancelada] = Cenario.LocacaoCancelada,
    };

    private static readonly Dictionary<string, Action<Locacao>> Eventos = new()
    {
        ["RegistrarPagamento"] = l => l.RegistrarPagamento(Cenario.Entrada(50m)),
        ["Prorrogar"] = l => l.Prorrogar(Cenario.Retirada.AddDays(1), Cenario.Prevista.AddDays(1)),
        ["CalcularPrevia"] = l => l.CalcularPrevia(Cenario.Prevista),
        ["Finalizar"] = l => l.Finalizar(
            Cenario.Prevista, Cenario.KmInicial + 500, null, [Cenario.Saida(Cenario.PrevistoTresDiarias)]),
        ["Cancelar"] = l => l.Cancelar(),
    };

    public static TheoryData<string, StatusLocacao> TransicoesValidas => new()
    {
        { "RegistrarPagamento", StatusLocacao.Aberta },
        { "Prorrogar", StatusLocacao.Aberta },
        { "CalcularPrevia", StatusLocacao.Aberta },
        { "Finalizar", StatusLocacao.Finalizada },
        { "Cancelar", StatusLocacao.Cancelada },
    };

    public static TheoryData<StatusLocacao, string> TransicoesInvalidas()
    {
        var dados = new TheoryData<StatusLocacao, string>();
        foreach (var estado in new[] { StatusLocacao.Finalizada, StatusLocacao.Cancelada })
            foreach (var evento in Eventos.Keys)
                dados.Add(estado, evento);
        return dados;
    }

    [Theory]
    [MemberData(nameof(TransicoesValidas))]
    public void LocacaoAberta_EventoPermitido_VaiParaEstadoEsperado(string evento, StatusLocacao esperado)
    {
        var locacao = Estados[StatusLocacao.Aberta]();

        Eventos[evento](locacao);

        Assert.Equal(esperado, locacao.Status);
    }

    [Theory]
    [MemberData(nameof(TransicoesInvalidas))]
    public void LocacaoEncerrada_QualquerEvento_LancaExcecaoEMantemEstado(StatusLocacao estado, string evento)
    {
        var locacao = Estados[estado]();
        var valorPago = locacao.ValorPago;

        Assert.Throws<DomainException>(() => Eventos[evento](locacao));

        Assert.Equal(estado, locacao.Status);
        Assert.Equal(valorPago, locacao.ValorPago);
    }

    [Fact]
    public void Abrir_VeiculoDisponivel_AbreLocacaoEAlocaVeiculo()
    {
        var veiculo = Cenario.Veiculo();

        var locacao = new Locacao(Cenario.Cliente(), veiculo, Cenario.Retirada, Cenario.Prevista, "  tanque cheio ");

        Assert.Equal(StatusLocacao.Aberta, locacao.Status);
        Assert.Equal(StatusVeiculo.Locado, veiculo.Status);
        Assert.Equal(7, locacao.ClienteId);
        Assert.Equal(9, locacao.VeiculoId);
        Assert.Equal(Cenario.KmInicial, locacao.KmRetirada);
        Assert.Equal(Cenario.Diaria, locacao.ValorDiaria);
        Assert.Equal(Cenario.PrevistoTresDiarias, locacao.ValorPrevisto);
        Assert.Equal(Cenario.PrevistoTresDiarias, locacao.ValorTotal);
        Assert.Equal(0m, locacao.ValorPago);
        Assert.Equal("tanque cheio", locacao.ObservacaoRetirada);
    }

    [Fact]
    public void Abrir_VeiculoJaLocado_LancaExcecao()
    {
        var veiculo = Cenario.Veiculo();
        _ = new Locacao(Cenario.Cliente(), veiculo, Cenario.Retirada, Cenario.Prevista, null);

        Assert.Throws<DomainException>(() =>
            new Locacao(Cenario.Cliente(), veiculo, Cenario.Retirada, Cenario.Prevista, null));
    }

    [Fact]
    public void Abrir_PrevistaIgualARetirada_LancaExcecaoSemAlocarVeiculo()
    {
        var veiculo = Cenario.Veiculo();

        Assert.Throws<DomainException>(() =>
            new Locacao(Cenario.Cliente(), veiculo, Cenario.Retirada, Cenario.Retirada, null));
        Assert.Equal(StatusVeiculo.Disponivel, veiculo.Status);
    }

    [Fact]
    public void Abrir_PeriodoMenorQueUmDia_CobraUmaDiaria()
    {
        var locacao = Cenario.LocacaoAberta(prevista: Cenario.Retirada.AddHours(2));

        Assert.Equal(1, locacao.DiariasContratadas());
        Assert.Equal(Cenario.Diaria, locacao.ValorPrevisto);
    }

    [Fact]
    public void Cancelar_LocacaoAberta_DevolveVeiculoParaDisponivel()
    {
        var locacao = Cenario.LocacaoAberta();

        locacao.Cancelar();

        Assert.Equal(StatusVeiculo.Disponivel, locacao.Veiculo.Status);
    }

    [Fact]
    public void Prorrogar_AntesDeVencer_RecalculaPrevistoComDiariaDaAbertura()
    {
        var locacao = Cenario.LocacaoAberta();
        locacao.Veiculo.Categoria.Atualizar("Econômico", 150m);

        locacao.Prorrogar(Cenario.Prevista, Cenario.Prevista.AddDays(2));

        Assert.Equal(Cenario.Prevista.AddDays(2), locacao.DataPrevistaDevolucao);
        Assert.Equal(500m, locacao.ValorPrevisto);
        Assert.Equal(500m, locacao.ValorTotal);
    }

    [Fact]
    public void Prorrogar_LocacaoVencida_LancaExcecao()
    {
        var locacao = Cenario.LocacaoAberta();

        Assert.Throws<DomainException>(() =>
            locacao.Prorrogar(Cenario.Prevista.AddMinutes(1), Cenario.Prevista.AddDays(2)));
    }

    [Fact]
    public void Prorrogar_NovaDataNaoPosteriorAPrevista_LancaExcecao()
    {
        var locacao = Cenario.LocacaoAberta();

        Assert.Throws<DomainException>(() => locacao.Prorrogar(Cenario.Retirada, Cenario.Prevista));
    }

    [Fact]
    public void RegistrarPagamento_EntradaIgualAoSaldo_QuitaLocacao()
    {
        var locacao = Cenario.LocacaoAberta();

        locacao.RegistrarPagamento(Cenario.Entrada(Cenario.PrevistoTresDiarias));

        Assert.Equal(0m, locacao.SaldoDevedor());
    }

    [Fact]
    public void RegistrarPagamento_EntradaAcimaDoSaldo_LancaExcecao()
    {
        var locacao = Cenario.LocacaoAberta();

        Assert.Throws<DomainException>(() => locacao.RegistrarPagamento(Cenario.Entrada(300.01m)));
    }

    // Saídas só existem no fechamento (Finalizar exige que sejam do tipo saída e fechem o saldo).
    // Registrada com a locação aberta, uma saída escapa da checagem de saldo e o deixa negativo.
    [Fact]
    public void RegistrarPagamento_SaidaComLocacaoAberta_LancaExcecao()
    {
        var locacao = Cenario.LocacaoAberta();

        Assert.Throws<DomainException>(() => locacao.RegistrarPagamento(Cenario.Saida(500m)));
        Assert.True(locacao.SaldoDevedor() >= 0);
    }
}
