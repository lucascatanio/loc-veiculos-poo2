using Locadora.Domain.Excecoes;
using Locadora.Domain.Tests.Apoio;

namespace Locadora.Domain.Tests.R1_DevolucaoComMulta;

// Condição: atraso em relação à data prevista (tolerância de 60 min).
//   devolveu antes ou na hora      -> 0 dias, sem multa
//   atraso <= 60 min               -> 0 dias, sem multa
//   atraso > 60 min                -> dias de atraso arredondados para cima, multa = dias × diária × 1,20
[Trait(Tecnica.Requisito, Tecnica.Devolucao)]
[Trait(Tecnica.Nome, Tecnica.TabelaDecisao)]
public class CalculoMultaTests
{
    [Theory]
    [InlineData(-120, 0, 0)]
    [InlineData(0, 0, 0)]
    [InlineData(60, 0, 0)]
    [InlineData(61, 1, 120)]
    [InlineData(24 * 60, 1, 120)]
    [InlineData(48 * 60, 2, 240)]
    public void CalcularMulta_ConformeMinutosDeAtraso(int minutosAposPrevista, int diasEsperados, int multaEsperada)
    {
        var locacao = Cenario.LocacaoAberta();
        var devolucao = Cenario.Prevista.AddMinutes(minutosAposPrevista);

        Assert.Equal(diasEsperados, locacao.CalcularDiasAtraso(devolucao));
        Assert.Equal(multaEsperada, locacao.CalcularMulta(devolucao));
        Assert.Equal(Cenario.PrevistoTresDiarias + multaEsperada, locacao.CalcularValorTotal(devolucao));
    }

    [Fact]
    public void CalcularMulta_DiariaComCentavos_ArredondaParaDuasCasas()
    {
        var locacao = Cenario.LocacaoAberta(diaria: 33.33m);

        Assert.Equal(40.00m, locacao.CalcularMulta(Cenario.Prevista.AddHours(2)));
    }

    [Fact]
    public void CalcularPrevia_LocacaoAbertaComEntrada_DetalhaValoresESaldo()
    {
        var locacao = Cenario.LocacaoAberta();
        locacao.RegistrarPagamento(Cenario.Entrada(100m));

        var previa = locacao.CalcularPrevia(Cenario.Prevista.AddMinutes(61));

        Assert.Equal(3, previa.DiariasContratadas);
        Assert.Equal(1, previa.DiasAtraso);
        Assert.Equal(300m, previa.ValorPrevisto);
        Assert.Equal(120m, previa.ValorMulta);
        Assert.Equal(420m, previa.ValorTotal);
        Assert.Equal(100m, previa.ValorPago);
        Assert.Equal(320m, previa.Saldo);
    }

    [Fact]
    public void CalcularPrevia_DevolucaoAntesDaRetirada_LancaExcecao()
    {
        var locacao = Cenario.LocacaoAberta();

        Assert.Throws<DomainException>(() => locacao.CalcularPrevia(Cenario.Retirada.AddDays(-1)));
    }
}
