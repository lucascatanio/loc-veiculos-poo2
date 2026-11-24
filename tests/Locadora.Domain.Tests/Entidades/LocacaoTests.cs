using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Tests.Suporte;
using Locadora.Domain.Valores;

namespace Locadora.Domain.Tests.Entidades;

public sealed class LocacaoTests
{
    private const string MensagemLimite = "O valor da locação passaria do limite de R$ 99.999.999,99. Reduza o período.";

    // 33.333.333,33 x 3 diárias = exatamente VALOR_MAXIMO; 99.999.999,99 x 1 também.
    public static TheoryData<decimal, int> PrevistoNoLimite => new()
    {
        { 33_333_333.33m, 3 },
        { 99_999_999.99m, 1 },
    };

    // 50.000.000,00 x 2 = VALOR_MAXIMO + 0,01.
    public static TheoryData<decimal, int> PrevistoAcimaDoLimite => new()
    {
        { 50_000_000m, 2 },
        { 33_333_333.34m, 3 },
    };

    public static TheoryData<decimal, int, decimal> MultaPorAtraso => new()
    {
        { 100m, 60, 0m },
        { 100m, 61, 120m },
        { 100m, 1440, 120m },
        { 100m, 1441, 240m },
        { 10.01m, 61, 12.01m },   // 12,012 arredonda para baixo
        { 10.02m, 61, 12.02m },   // 12,024
        { 10.03m, 61, 12.04m },   // 12,036 arredonda para cima
        { 10.04m, 61, 12.05m },   // 12,048
        { 0.01m, 61, 0.01m },     // 0,012
        { 0.03m, 61, 0.04m },     // 0,036
        { 33.33m, 2881, 119.99m }, // 3 dias: 119,988
    };

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1440)]
    public void Construtor_PrevistaIgualOuAnteriorARetirada_Lanca(int minutosAposRetirada)
    {
        var ex = Assert.Throws<DomainException>(() =>
            Fabrica.Locacao(retirada: Fabrica.Agora, prevista: Fabrica.Agora.AddMinutes(minutosAposRetirada)));

        Assert.Equal("A data prevista de devolução deve ser posterior à retirada.", ex.Message);
    }

    [Fact]
    public void Construtor_DadosValidos_CopiaClienteEVeiculo()
    {
        var cliente = Fabrica.Cliente(id: 7);
        var veiculo = Fabrica.Veiculo(id: 9);

        var locacao = Fabrica.Locacao(cliente: cliente, veiculo: veiculo);

        Assert.Same(cliente, locacao.Cliente);
        Assert.Equal(7, locacao.ClienteId);
        Assert.Same(veiculo, locacao.Veiculo);
        Assert.Equal(9, locacao.VeiculoId);
    }

    [Fact]
    public void Construtor_DadosValidos_PreencheValoresIniciais()
    {
        var retirada = Fabrica.Agora;
        var prevista = retirada.AddDays(3);

        var locacao = Fabrica.Locacao(valorDiaria: 150m, kmRetirada: 54_321, retirada: retirada, prevista: prevista);

        Assert.Equal(retirada, locacao.DataRetirada);
        Assert.Equal(prevista, locacao.DataPrevistaDevolucao);
        Assert.Null(locacao.DataDevolucao);
        Assert.Equal(54_321, locacao.KmRetirada);
        Assert.Null(locacao.KmDevolucao);
        Assert.Equal(150m, locacao.ValorDiaria);
        Assert.Equal(450m, locacao.ValorPrevisto);
        Assert.Equal(450m, locacao.ValorTotal);
        Assert.Equal(0m, locacao.ValorPago);
        Assert.Equal(0m, locacao.ValorMulta);
        Assert.Equal(StatusLocacao.Aberta, locacao.Status);
        Assert.Empty(locacao.Pagamentos);
    }

    [Fact]
    public void Construtor_VeiculoDisponivel_AlocaVeiculo()
    {
        var veiculo = Fabrica.Veiculo();

        Fabrica.Locacao(veiculo: veiculo);

        Assert.Equal(StatusVeiculo.Locado, veiculo.Status);
    }

    [Fact]
    public void Construtor_ObservacaoComEspacos_GuardaComTrim()
    {
        var locacao = Fabrica.Locacao(observacao: "  arranhão no para-choque  ");

        Assert.Equal("arranhão no para-choque", locacao.ObservacaoRetirada);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ObservacaoEmBranco_GuardaNulo(string? observacao)
    {
        var locacao = Fabrica.Locacao(observacao: observacao);

        Assert.Null(locacao.ObservacaoRetirada);
    }

    [Theory]
    [InlineData(StatusVeiculo.Locado, true)]
    [InlineData(StatusVeiculo.Manutencao, true)]
    [InlineData(StatusVeiculo.Disponivel, false)]
    public void Construtor_VeiculoIndisponivel_Lanca(StatusVeiculo status, bool ativo)
    {
        var veiculo = Fabrica.Veiculo(status: status, ativo: ativo);

        var ex = Assert.Throws<DomainException>(() => Fabrica.Locacao(veiculo: veiculo));

        Assert.Equal("Veículo indisponível para locação.", ex.Message);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(60, 1)]
    [InlineData(1440, 1)]
    [InlineData(1441, 2)]
    [InlineData(2880, 2)]
    [InlineData(2881, 3)]
    [InlineData(4320, 3)]
    public void DiariasContratadas_PorDuracao_ArredondaParaCimaComMinimoDeUma(int minutos, int esperadas)
    {
        var locacao = Fabrica.Locacao(prevista: Fabrica.Agora.AddMinutes(minutos));

        Assert.Equal(esperadas, locacao.DiariasContratadas());
    }

    [Theory]
    [InlineData(60, 100)]
    [InlineData(1440, 100)]
    [InlineData(1441, 200)]
    [InlineData(2880, 200)]
    public void ValorPrevisto_PorDuracao_MultiplicaDiariasPelaDiaria(int minutos, int esperado)
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddMinutes(minutos));

        Assert.Equal(esperado, locacao.ValorPrevisto);
        Assert.Equal(esperado, locacao.ValorTotal);
    }

    [Fact]
    public void ValorPrevisto_DiariaComCentavos_MultiplicaPelasDiarias()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 33.33m, prevista: Fabrica.Agora.AddDays(3));

        Assert.Equal(99.99m, locacao.ValorPrevisto);
    }

    [Theory]
    [MemberData(nameof(PrevistoNoLimite))]
    public void Construtor_PrevistoExatamenteNoLimite_Aceita(decimal valorDiaria, int dias)
    {
        var locacao = Fabrica.Locacao(valorDiaria: valorDiaria, prevista: Fabrica.Agora.AddDays(dias));

        Assert.Equal(Locacao.VALOR_MAXIMO, locacao.ValorPrevisto);
    }

    [Theory]
    [MemberData(nameof(PrevistoAcimaDoLimite))]
    public void Construtor_PrevistoAcimaDoLimite_Lanca(decimal valorDiaria, int dias)
    {
        var ex = Assert.Throws<DomainException>(() =>
            Fabrica.Locacao(valorDiaria: valorDiaria, prevista: Fabrica.Agora.AddDays(dias)));

        Assert.Equal(MensagemLimite, ex.Message);
    }

    [Fact]
    public void Construtor_PrevistoAcimaDoLimite_NaoAlocaVeiculo()
    {
        var veiculo = Fabrica.Veiculo(categoria: Fabrica.Categoria(valorDiaria: 50_000_000m));

        var ex = Assert.Throws<DomainException>(
            () => Fabrica.Locacao(veiculo: veiculo, prevista: Fabrica.Agora.AddDays(2)));

        Assert.Equal(MensagemLimite, ex.Message);
        Assert.Equal(StatusVeiculo.Disponivel, veiculo.Status);
    }

    [Theory]
    [InlineData(-1440, 0)]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(60, 0)]
    [InlineData(61, 1)]
    [InlineData(1440, 1)]
    [InlineData(1441, 2)]
    [InlineData(2880, 2)]
    [InlineData(2881, 3)]
    public void CalcularDiasAtraso_PorMinutosAposAPrevista_AplicaToleranciaEArredondaParaCima(int minutos, int esperados)
    {
        var locacao = Fabrica.Locacao();

        var dias = locacao.CalcularDiasAtraso(locacao.DataPrevistaDevolucao.AddMinutes(minutos));

        Assert.Equal(esperados, dias);
    }

    [Fact]
    public void CalcularDiasAtraso_ToleranciaMaisUmSegundo_CobraUmDia()
    {
        var locacao = Fabrica.Locacao();

        var dias = locacao.CalcularDiasAtraso(locacao.DataPrevistaDevolucao.AddMinutes(60).AddSeconds(1));

        Assert.Equal(1, dias);
    }

    [Theory]
    [MemberData(nameof(MultaPorAtraso))]
    public void CalcularMulta_PorAtraso_CobraDiasVezesDiariaMaisVintePorCento(
        decimal valorDiaria, int minutosAtraso, decimal esperada)
    {
        var locacao = Fabrica.Locacao(valorDiaria: valorDiaria);

        var multa = locacao.CalcularMulta(locacao.DataPrevistaDevolucao.AddMinutes(minutosAtraso));

        Assert.Equal(esperada, multa);
    }

    [Theory]
    [InlineData(0, 300)]
    [InlineData(60, 300)]
    [InlineData(61, 420)]
    [InlineData(1441, 540)]
    public void CalcularValorTotal_PorAtraso_SomaPrevistoEMulta(int minutosAtraso, int esperado)
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));

        var total = locacao.CalcularValorTotal(locacao.DataPrevistaDevolucao.AddMinutes(minutosAtraso));

        Assert.Equal(esperado, total);
    }

    [Fact]
    public void CalcularPrevia_LocacaoAbertaComAtrasoEEntrada_PreencheTodosOsCampos()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));
        locacao.RegistrarPagamento(Fabrica.Entrada(50m));

        var previa = locacao.CalcularPrevia(locacao.DataPrevistaDevolucao.AddMinutes(1441));

        Assert.Equal(new PreviaDevolucao(
            DiariasContratadas: 3, DiasAtraso: 2, ValorPrevisto: 300m, ValorMulta: 240m,
            ValorTotal: 540m, ValorPago: 50m, Saldo: 490m), previa);
    }

    [Fact]
    public void CalcularPrevia_DevolucaoNoPrazo_NaoCobraMulta()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));

        var previa = locacao.CalcularPrevia(locacao.DataPrevistaDevolucao);

        Assert.Equal(new PreviaDevolucao(3, 0, 300m, 0m, 300m, 0m, 300m), previa);
    }

    [Fact]
    public void CalcularPrevia_LocacaoFinalizada_Lanca()
    {
        var locacao = Finalizada();

        var ex = Assert.Throws<DomainException>(() => locacao.CalcularPrevia(locacao.DataPrevistaDevolucao));

        Assert.Equal("Só é possível calcular a prévia de locação aberta.", ex.Message);
    }

    [Fact]
    public void CalcularPrevia_LocacaoCancelada_Lanca()
    {
        var locacao = Fabrica.Locacao();
        locacao.Cancelar();

        var ex = Assert.Throws<DomainException>(() => locacao.CalcularPrevia(locacao.DataPrevistaDevolucao));

        Assert.Equal("Só é possível calcular a prévia de locação aberta.", ex.Message);
    }

    [Fact]
    public void SaldoDevedor_SemPagamentos_EhOValorTotal()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));

        Assert.Equal(300m, locacao.SaldoDevedor());
    }

    [Fact]
    public void SaldoDevedor_AposEntradas_DescontaOPago()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));
        locacao.RegistrarPagamento(Fabrica.Entrada(100m));
        locacao.RegistrarPagamento(Fabrica.Entrada(50.50m));

        Assert.Equal(149.50m, locacao.SaldoDevedor());
    }

    [Fact]
    public void RegistrarPagamento_EntradaIgualAoSaldo_Aceita()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));

        locacao.RegistrarPagamento(Fabrica.Entrada(300m));

        Assert.Equal(0m, locacao.SaldoDevedor());
    }

    [Fact]
    public void RegistrarPagamento_EntradaUmCentavoAcimaDoSaldo_Lanca()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));

        var ex = Assert.Throws<DomainException>(() => locacao.RegistrarPagamento(Fabrica.Entrada(300.01m)));

        Assert.Equal("O pagamento excede o saldo devedor de R$ 300,00.", ex.Message);
        Assert.Empty(locacao.Pagamentos);
        Assert.Equal(0m, locacao.ValorPago);
    }

    [Fact]
    public void RegistrarPagamento_EntradaAcimaDoSaldoRestante_FormataSaldoEmPtBr()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 1_000m, prevista: Fabrica.Agora.AddDays(5));
        locacao.RegistrarPagamento(Fabrica.Entrada(1_999.99m));

        var ex = Assert.Throws<DomainException>(() => locacao.RegistrarPagamento(Fabrica.Entrada(3_000.02m)));

        Assert.Equal("O pagamento excede o saldo devedor de R$ 3.000,01.", ex.Message);
    }

    [Fact]
    public void RegistrarPagamento_VariosPagamentos_AcumulaValorPagoEAdicionaNaLista()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));
        var primeiro = Fabrica.Entrada(100m);
        var segundo = Fabrica.Entrada(75.25m);

        locacao.RegistrarPagamento(primeiro);
        locacao.RegistrarPagamento(segundo);

        Assert.Equal(175.25m, locacao.ValorPago);
        Assert.Equal(new[] { primeiro, segundo }, locacao.Pagamentos);
    }

    [Fact]
    public void RegistrarPagamento_LocacaoFinalizada_Lanca()
    {
        var locacao = Finalizada();

        var ex = Assert.Throws<DomainException>(() => locacao.RegistrarPagamento(Fabrica.Entrada(10m)));

        Assert.Equal("Só é possível registrar pagamento em locação aberta.", ex.Message);
    }

    [Fact]
    public void RegistrarPagamento_LocacaoCancelada_Lanca()
    {
        var locacao = Fabrica.Locacao();
        locacao.Cancelar();

        var ex = Assert.Throws<DomainException>(() => locacao.RegistrarPagamento(Fabrica.Entrada(10m)));

        Assert.Equal("Só é possível registrar pagamento em locação aberta.", ex.Message);
    }

    [Fact]
    public void Finalizar_LocacaoJaFinalizada_Lanca()
    {
        var locacao = Finalizada();

        var ex = Assert.Throws<DomainException>(
            () => locacao.Finalizar(locacao.DataPrevistaDevolucao, 20_000, null, []));

        Assert.Equal("Só é possível finalizar locação aberta.", ex.Message);
    }

    [Fact]
    public void Finalizar_LocacaoCancelada_Lanca()
    {
        var locacao = Fabrica.Locacao();
        locacao.Cancelar();

        var ex = Assert.Throws<DomainException>(
            () => locacao.Finalizar(locacao.DataPrevistaDevolucao, 20_000, null, []));

        Assert.Equal("Só é possível finalizar locação aberta.", ex.Message);
    }

    [Fact]
    public void Finalizar_KmMenorQueORetirada_Lanca()
    {
        var locacao = Fabrica.Locacao(kmRetirada: 10_000);

        var ex = Assert.Throws<DomainException>(
            () => locacao.Finalizar(locacao.DataPrevistaDevolucao, 9_999, null, []));

        Assert.Equal("Km de devolução menor que o de retirada.", ex.Message);
    }

    [Fact]
    public void Finalizar_KmIgualAoDaRetirada_Aceita()
    {
        var locacao = Locacao3DiasSaldoZerado(kmRetirada: 10_000);

        locacao.Finalizar(locacao.DataPrevistaDevolucao, 10_000, null, []);

        Assert.Equal(10_000, locacao.KmDevolucao);
    }

    [Fact]
    public void Finalizar_PagamentoDoTipoEntradaNaLista_Lanca()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));

        var ex = Assert.Throws<DomainException>(
            () => locacao.Finalizar(locacao.DataPrevistaDevolucao, 10_500, null, [Fabrica.Entrada(300m)]));

        Assert.Equal("Os pagamentos da finalização devem ser do tipo saída.", ex.Message);
    }

    [Fact]
    public void Finalizar_SaidasFechamOSaldoNoPrazo_Aceita()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));
        var saidas = new[] { Fabrica.Saida(100m), Fabrica.Saida(200m) };

        locacao.Finalizar(locacao.DataPrevistaDevolucao, 10_500, null, saidas);

        Assert.Equal(StatusLocacao.Finalizada, locacao.Status);
        Assert.Equal(300m, locacao.ValorPago);
    }

    [Fact]
    public void Finalizar_SaidasFechamOSaldoComMulta_Aceita()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));
        var devolucao = locacao.DataPrevistaDevolucao.AddMinutes(61);

        locacao.Finalizar(devolucao, 10_500, null, [Fabrica.Saida(420m)]);

        Assert.Equal(StatusLocacao.Finalizada, locacao.Status);
        Assert.Equal(120m, locacao.ValorMulta);
        Assert.Equal(420m, locacao.ValorTotal);
    }

    [Fact]
    public void Finalizar_SaidasFechamOSaldoAposEntradaPrevia_Aceita()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));
        locacao.RegistrarPagamento(Fabrica.Entrada(100m));
        var devolucao = locacao.DataPrevistaDevolucao.AddMinutes(61);

        locacao.Finalizar(devolucao, 10_500, null, [Fabrica.Saida(320m)]);

        Assert.Equal(StatusLocacao.Finalizada, locacao.Status);
        Assert.Equal(420m, locacao.ValorPago);
        Assert.Equal(0m, locacao.SaldoDevedor());
    }

    [Fact]
    public void Finalizar_SaldoZeroComListaVazia_Aceita()
    {
        var locacao = Locacao3DiasSaldoZerado();

        locacao.Finalizar(locacao.DataPrevistaDevolucao, 10_500, null, []);

        Assert.Equal(StatusLocacao.Finalizada, locacao.Status);
    }

    [Fact]
    public void Finalizar_SaidasInsuficientes_LancaComValorQueFalta()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));

        var ex = Assert.Throws<DomainException>(
            () => locacao.Finalizar(locacao.DataPrevistaDevolucao, 10_500, null, [Fabrica.Saida(100m)]));

        Assert.Equal("Os pagamentos de saída devem fechar o saldo de R$ 300,00. Falta R$ 200,00.", ex.Message);
    }

    [Fact]
    public void Finalizar_SaidasExcedemOSaldo_LancaComValorQueSobra()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));

        var ex = Assert.Throws<DomainException>(
            () => locacao.Finalizar(locacao.DataPrevistaDevolucao, 10_500, null, [Fabrica.Saida(400m)]));

        Assert.Equal("Os pagamentos de saída devem fechar o saldo de R$ 300,00. Sobra R$ 100,00.", ex.Message);
    }

    [Fact]
    public void Finalizar_ListaVaziaComSaldoPendente_LancaFaltaOSaldoTodo()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));
        locacao.RegistrarPagamento(Fabrica.Entrada(100m));

        var ex = Assert.Throws<DomainException>(
            () => locacao.Finalizar(locacao.DataPrevistaDevolucao, 10_500, null, []));

        Assert.Equal("Os pagamentos de saída devem fechar o saldo de R$ 200,00. Falta R$ 200,00.", ex.Message);
    }

    [Fact]
    public void Finalizar_DiferencaDeUmCentavo_LancaComFormatoPtBr()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 1_000m, prevista: Fabrica.Agora.AddDays(2));

        var falta = Assert.Throws<DomainException>(
            () => locacao.Finalizar(locacao.DataPrevistaDevolucao, 10_500, null, [Fabrica.Saida(1_999.99m)]));
        var sobra = Assert.Throws<DomainException>(
            () => locacao.Finalizar(locacao.DataPrevistaDevolucao, 10_500, null, [Fabrica.Saida(2_000.01m)]));

        Assert.Equal("Os pagamentos de saída devem fechar o saldo de R$ 2.000,00. Falta R$ 0,01.", falta.Message);
        Assert.Equal("Os pagamentos de saída devem fechar o saldo de R$ 2.000,00. Sobra R$ 0,01.", sobra.Message);
    }

    [Fact]
    public void Finalizar_DadosValidos_AtualizaLocacao()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));
        var entrada = Fabrica.Entrada(100m);
        locacao.RegistrarPagamento(entrada);
        var saida = Fabrica.Saida(320m);
        var devolucao = locacao.DataPrevistaDevolucao.AddMinutes(61);

        locacao.Finalizar(devolucao, 10_750, "  sem avarias  ", [saida]);

        Assert.Equal(StatusLocacao.Finalizada, locacao.Status);
        Assert.Equal(devolucao, locacao.DataDevolucao);
        Assert.Equal(10_750, locacao.KmDevolucao);
        Assert.Equal("sem avarias", locacao.ObservacaoDevolucao);
        Assert.Equal(120m, locacao.ValorMulta);
        Assert.Equal(420m, locacao.ValorTotal);
        Assert.Equal(420m, locacao.ValorPago);
        Assert.Equal(new[] { entrada, saida }, locacao.Pagamentos);
    }

    [Fact]
    public void Finalizar_DadosValidos_LiberaVeiculoComNovoKm()
    {
        var veiculo = Fabrica.Veiculo(kmAtual: 10_000);
        var locacao = Fabrica.Locacao(veiculo: veiculo);
        locacao.RegistrarPagamento(Fabrica.Entrada(300m));

        locacao.Finalizar(locacao.DataPrevistaDevolucao, 10_750, null, []);

        Assert.Equal(StatusVeiculo.Disponivel, veiculo.Status);
        Assert.Equal(10_750, veiculo.KmAtual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Finalizar_ObservacaoEmBranco_GuardaNulo(string? observacao)
    {
        var locacao = Locacao3DiasSaldoZerado();

        locacao.Finalizar(locacao.DataPrevistaDevolucao, 10_500, observacao, []);

        Assert.Null(locacao.ObservacaoDevolucao);
    }

    [Theory]
    [MemberData(nameof(MultaPorAtraso))]
    public void Finalizar_ComAtraso_GravaMultaCalculadaPelaDataDeDevolucao(
        decimal valorDiaria, int minutosAtraso, decimal multaEsperada)
    {
        var locacao = Fabrica.Locacao(valorDiaria: valorDiaria, prevista: Fabrica.Agora.AddDays(1));
        var devolucao = locacao.DataPrevistaDevolucao.AddMinutes(minutosAtraso);
        var saida = Fabrica.Saida(valorDiaria + multaEsperada);

        locacao.Finalizar(devolucao, 10_500, null, [saida]);

        Assert.Equal(multaEsperada, locacao.ValorMulta);
        Assert.Equal(valorDiaria + multaEsperada, locacao.ValorTotal);
    }

    [Fact]
    public void Finalizar_TotalComMultaExatamenteNoLimite_Aceita()
    {
        // 45.454.545,45 de previsto + 54.545.454,54 de multa (1 dia) = VALOR_MAXIMO
        var locacao = Fabrica.Locacao(valorDiaria: 45_454_545.45m, prevista: Fabrica.Agora.AddDays(1));
        var devolucao = locacao.DataPrevistaDevolucao.AddMinutes(61);

        locacao.Finalizar(devolucao, 10_500, null, [Fabrica.Saida(Locacao.VALOR_MAXIMO)]);

        Assert.Equal(Locacao.VALOR_MAXIMO, locacao.ValorTotal);
    }

    [Fact]
    public void Finalizar_TotalComMultaAcimaDoLimite_Lanca()
    {
        // 45.454.545,46 + 54.545.454,55 = 100.000.000,01: o primeiro total acima do limite com 1 dia de atraso
        var locacao = Fabrica.Locacao(valorDiaria: 45_454_545.46m, prevista: Fabrica.Agora.AddDays(1));
        var devolucao = locacao.DataPrevistaDevolucao.AddMinutes(61);

        var ex = Assert.Throws<DomainException>(() => locacao.Finalizar(devolucao, 10_500, null, []));

        Assert.Equal(MensagemLimite, ex.Message);
        AssertNaoFinalizada(locacao);
    }

    [Fact]
    public void Finalizar_KmMenorQueORetirada_NaoAlteraEstado()
    {
        var locacao = Fabrica.Locacao(kmRetirada: 10_000);

        var ex = Assert.Throws<DomainException>(
            () => locacao.Finalizar(locacao.DataPrevistaDevolucao, 9_999, "obs", []));

        Assert.Equal("Km de devolução menor que o de retirada.", ex.Message);
        AssertNaoFinalizada(locacao);
    }

    [Fact]
    public void Finalizar_PagamentoDoTipoEntradaNaLista_NaoAlteraEstado()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));

        var ex = Assert.Throws<DomainException>(
            () => locacao.Finalizar(locacao.DataPrevistaDevolucao, 10_500, "obs", [Fabrica.Entrada(300m)]));

        Assert.Equal("Os pagamentos da finalização devem ser do tipo saída.", ex.Message);
        AssertNaoFinalizada(locacao);
    }

    [Fact]
    public void Finalizar_SaldoNaoFechado_NaoAlteraEstado()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));

        var ex = Assert.Throws<DomainException>(
            () => locacao.Finalizar(locacao.DataPrevistaDevolucao.AddMinutes(61), 10_500, "obs", [Fabrica.Saida(10m)]));

        Assert.Equal("Os pagamentos de saída devem fechar o saldo de R$ 420,00. Falta R$ 410,00.", ex.Message);
        AssertNaoFinalizada(locacao);
    }

    [Fact]
    public void Prorrogar_LocacaoFinalizada_Lanca()
    {
        var locacao = Finalizada();

        var ex = Assert.Throws<DomainException>(
            () => locacao.Prorrogar(Fabrica.Agora, locacao.DataPrevistaDevolucao.AddDays(1)));

        Assert.Equal("Só é possível prorrogar locação aberta.", ex.Message);
    }

    [Fact]
    public void Prorrogar_LocacaoCancelada_Lanca()
    {
        var locacao = Fabrica.Locacao();
        locacao.Cancelar();

        var ex = Assert.Throws<DomainException>(
            () => locacao.Prorrogar(Fabrica.Agora, locacao.DataPrevistaDevolucao.AddDays(1)));

        Assert.Equal("Só é possível prorrogar locação aberta.", ex.Message);
    }

    [Fact]
    public void Prorrogar_AgoraDepoisDaPrevista_Lanca()
    {
        var locacao = Fabrica.Locacao();
        var agora = locacao.DataPrevistaDevolucao.AddMinutes(1);

        var ex = Assert.Throws<DomainException>(() => locacao.Prorrogar(agora, agora.AddDays(1)));

        Assert.Equal("Locação vencida não pode ser prorrogada.", ex.Message);
    }

    [Fact]
    public void Prorrogar_AgoraIgualAPrevista_Aceita()
    {
        var locacao = Fabrica.Locacao();
        var novaData = locacao.DataPrevistaDevolucao.AddDays(1);

        locacao.Prorrogar(locacao.DataPrevistaDevolucao, novaData);

        Assert.Equal(novaData, locacao.DataPrevistaDevolucao);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1440)]
    public void Prorrogar_NovaDataIgualOuAnteriorAPrevista_Lanca(int minutosAposPrevista)
    {
        var locacao = Fabrica.Locacao();
        var novaData = locacao.DataPrevistaDevolucao.AddMinutes(minutosAposPrevista);

        var ex = Assert.Throws<DomainException>(() => locacao.Prorrogar(Fabrica.Agora, novaData));

        Assert.Equal("A nova data deve ser posterior à data prevista atual.", ex.Message);
    }

    [Theory]
    [InlineData(1, 400)]
    [InlineData(1440, 400)]
    [InlineData(1441, 500)]
    [InlineData(2880, 500)]
    public void Prorrogar_NovaData_RecalculaPrevistoETotal(int minutosAposPrevista, int previstoEsperado)
    {
        // retirada + 3 dias: prorrogar 1 minuto já abre a 4ª diária
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));
        var novaData = locacao.DataPrevistaDevolucao.AddMinutes(minutosAposPrevista);

        locacao.Prorrogar(Fabrica.Agora, novaData);

        Assert.Equal(novaData, locacao.DataPrevistaDevolucao);
        Assert.Equal(previstoEsperado, locacao.ValorPrevisto);
        Assert.Equal(previstoEsperado, locacao.ValorTotal);
        Assert.Equal(0m, locacao.ValorMulta);
    }

    [Fact]
    public void Prorrogar_ComEntradaPrevia_AumentaOSaldoDevedor()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, prevista: Fabrica.Agora.AddDays(3));
        locacao.RegistrarPagamento(Fabrica.Entrada(300m));

        locacao.Prorrogar(Fabrica.Agora, locacao.DataPrevistaDevolucao.AddDays(2));

        Assert.Equal(200m, locacao.SaldoDevedor());
    }

    [Fact]
    public void Prorrogar_NovoPrevistoExatamenteNoLimite_Aceita()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 33_333_333.33m, prevista: Fabrica.Agora.AddDays(2));

        locacao.Prorrogar(Fabrica.Agora, Fabrica.Agora.AddDays(3));

        Assert.Equal(Locacao.VALOR_MAXIMO, locacao.ValorPrevisto);
    }

    [Fact]
    public void Prorrogar_NovoPrevistoAcimaDoLimite_LancaENaoAlteraEstado()
    {
        var locacao = Fabrica.Locacao(valorDiaria: 50_000_000m, prevista: Fabrica.Agora.AddDays(1));
        var prevista = locacao.DataPrevistaDevolucao;

        var ex = Assert.Throws<DomainException>(() => locacao.Prorrogar(Fabrica.Agora, Fabrica.Agora.AddDays(2)));

        Assert.Equal(MensagemLimite, ex.Message);
        Assert.Equal(prevista, locacao.DataPrevistaDevolucao);
        Assert.Equal(50_000_000m, locacao.ValorPrevisto);
        Assert.Equal(50_000_000m, locacao.ValorTotal);
    }

    [Fact]
    public void Cancelar_LocacaoAberta_CancelaELiberaVeiculo()
    {
        var veiculo = Fabrica.Veiculo(kmAtual: 10_000);
        var locacao = Fabrica.Locacao(veiculo: veiculo);

        locacao.Cancelar();

        Assert.Equal(StatusLocacao.Cancelada, locacao.Status);
        Assert.Equal(StatusVeiculo.Disponivel, veiculo.Status);
        Assert.Equal(10_000, veiculo.KmAtual);
    }

    [Fact]
    public void Cancelar_LocacaoJaCancelada_Lanca()
    {
        var locacao = Fabrica.Locacao();
        locacao.Cancelar();

        var ex = Assert.Throws<DomainException>(locacao.Cancelar);

        Assert.Equal("Só é possível cancelar locação aberta.", ex.Message);
    }

    [Fact]
    public void Cancelar_LocacaoFinalizada_Lanca()
    {
        var locacao = Finalizada();

        var ex = Assert.Throws<DomainException>(locacao.Cancelar);

        Assert.Equal("Só é possível cancelar locação aberta.", ex.Message);
    }

    // 3 diárias de 100 já quitadas por entrada, então Finalizar no prazo aceita lista vazia.
    private static Locacao Locacao3DiasSaldoZerado(int kmRetirada = 10_000)
    {
        var locacao = Fabrica.Locacao(valorDiaria: 100m, kmRetirada: kmRetirada, prevista: Fabrica.Agora.AddDays(3));
        locacao.RegistrarPagamento(Fabrica.Entrada(300m));
        return locacao;
    }

    private static Locacao Finalizada()
    {
        var locacao = Locacao3DiasSaldoZerado();
        locacao.Finalizar(locacao.DataPrevistaDevolucao, 10_500, null, []);
        return locacao;
    }

    private static void AssertNaoFinalizada(Locacao locacao)
    {
        Assert.Equal(StatusLocacao.Aberta, locacao.Status);
        Assert.Equal(StatusVeiculo.Locado, locacao.Veiculo.Status);
        Assert.Equal(locacao.KmRetirada, locacao.Veiculo.KmAtual);
        Assert.Null(locacao.DataDevolucao);
        Assert.Null(locacao.KmDevolucao);
        Assert.Null(locacao.ObservacaoDevolucao);
        Assert.Equal(0m, locacao.ValorMulta);
        Assert.Equal(locacao.ValorPrevisto, locacao.ValorTotal);
        Assert.Equal(0m, locacao.ValorPago);
        Assert.Empty(locacao.Pagamentos);
    }
}
