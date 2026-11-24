using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Servicos;
using Locadora.Domain.Tests.Suporte;
using Locadora.Domain.Valores;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Locadora.Domain.Tests.Servicos;

public sealed class LocacaoServiceTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static readonly DateTime Prevista = Fabrica.Agora.AddDays(3);

    private readonly ILocacaoRepository _locacoes = Substitute.For<ILocacaoRepository>();
    private readonly IClienteService _clienteService = Substitute.For<IClienteService>();
    private readonly IVeiculoService _veiculoService = Substitute.For<IVeiculoService>();
    private readonly LocacaoService _servico;

    public LocacaoServiceTests() => _servico = new LocacaoService(_locacoes, _clienteService, _veiculoService);

    // 3 diárias de R$ 100, retirada em Fabrica.Agora: valor previsto R$ 300.
    private Locacao LocacaoAbertaNoRepositorio(int id = 5)
    {
        var locacao = Fabrica.Locacao(id: id);
        _locacoes.ObterComDetalhesAsync(id, Ct).Returns(locacao);
        return locacao;
    }

    [Fact]
    public async Task ObterAsync_LocacaoExiste_DevolveAMesmaInstanciaERepassaOToken()
    {
        var locacao = Fabrica.Locacao(id: 5);
        _locacoes.ObterComDetalhesAsync(5, Ct).Returns(locacao);

        var resultado = await _servico.ObterAsync(5, Ct);

        Assert.Same(locacao, resultado);
        await _locacoes.Received(1).ObterComDetalhesAsync(5, Ct);
    }

    [Fact]
    public async Task ObterAsync_LocacaoNaoExiste_LancaNaoEncontrado()
    {
        _locacoes.ObterComDetalhesAsync(5, Ct).Returns((Locacao?)null);

        var ex = await Assert.ThrowsAsync<NaoEncontradoException>(() => _servico.ObterAsync(5, Ct));

        Assert.Equal("Locação 5 não encontrado(a).", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ListarAsync_StatusAusente_FiltraPorAberta(string? status)
    {
        var lista = new List<Locacao> { Fabrica.Locacao() };
        _locacoes.ListarAsync(StatusLocacao.Aberta, Ct).Returns(lista);

        var resultado = await _servico.ListarAsync(status, Ct);

        Assert.Same(lista, resultado);
    }

    [Theory]
    [InlineData("finalizada", StatusLocacao.Finalizada)]
    [InlineData("Cancelada", StatusLocacao.Cancelada)]
    public async Task ListarAsync_StatusInformado_ConverteParaOEnum(string status, StatusLocacao esperado)
    {
        _locacoes.ListarAsync(Arg.Any<StatusLocacao>(), Ct).Returns([]);

        await _servico.ListarAsync(status, Ct);

        await _locacoes.Received(1).ListarAsync(esperado, Ct);
    }

    [Fact]
    public async Task ListarAsync_StatusInvalido_LancaDomainExceptionSemConsultarORepositorio()
    {
        var ex = await Assert.ThrowsAsync<DomainException>(() => _servico.ListarAsync("arquivada", Ct));

        Assert.Equal(
            "Valor \"arquivada\" inválido para status. Use: Aberta, Finalizada, Cancelada.", ex.Message);
        await _locacoes.DidNotReceive().ListarAsync(Arg.Any<StatusLocacao>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AbrirAsync_DadosValidos_DevolveLocacaoAbertaComClienteVeiculoEDatas()
    {
        var cliente = Fabrica.Cliente(id: 2);
        var veiculo = Fabrica.Veiculo(id: 3);
        _clienteService.ObterAsync(2, Ct).Returns(cliente);
        _veiculoService.ObterAsync(3, Ct).Returns(veiculo);

        var locacao = await _servico.AbrirAsync(2, 3, Fabrica.Agora, Prevista, "  Tanque cheio  ", [], Ct);

        Assert.Equal(StatusLocacao.Aberta, locacao.Status);
        Assert.Same(cliente, locacao.Cliente);
        Assert.Same(veiculo, locacao.Veiculo);
        Assert.Equal(Fabrica.Agora, locacao.DataRetirada);
        Assert.Equal(Prevista, locacao.DataPrevistaDevolucao);
        Assert.Equal("Tanque cheio", locacao.ObservacaoRetirada);
        Assert.Empty(locacao.Pagamentos);
        Assert.Equal(StatusVeiculo.Locado, veiculo.Status);
    }

    [Fact]
    public async Task AbrirAsync_ComEntradas_ViramPagamentosDeEntradaDatadosNaRetirada()
    {
        _clienteService.ObterAsync(2, Ct).Returns(Fabrica.Cliente(id: 2));
        _veiculoService.ObterAsync(3, Ct).Returns(Fabrica.Veiculo(id: 3));
        PagamentoInformado[] entradas = [new(100m, "pix"), new(50m, "CartaoDebito")];

        var locacao = await _servico.AbrirAsync(2, 3, Fabrica.Agora, Prevista, null, entradas, Ct);

        var pagamentos = locacao.Pagamentos.ToList();
        Assert.Equal(2, pagamentos.Count);
        Assert.All(pagamentos, p =>
        {
            Assert.Equal(TipoPagamento.Entrada, p.Tipo);
            Assert.Equal(Fabrica.Agora, p.DataPagamento);
        });
        Assert.Equal(100m, pagamentos[0].Valor);
        Assert.Equal(FormaPagamento.Pix, pagamentos[0].Forma);
        Assert.Equal(50m, pagamentos[1].Valor);
        Assert.Equal(FormaPagamento.CartaoDebito, pagamentos[1].Forma);
        Assert.Equal(150m, locacao.ValorPago);
    }

    [Fact]
    public async Task AbrirAsync_ClienteInativo_LancaDomainExceptionSemBuscarOVeiculo()
    {
        _clienteService.ObterAsync(2, Ct).Returns(Fabrica.Cliente(id: 2, ativo: false));

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _servico.AbrirAsync(2, 3, Fabrica.Agora, Prevista, null, [], Ct));

        Assert.Equal("O cliente está inativo.", ex.Message);
        await _veiculoService.DidNotReceive().ObterAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(StatusVeiculo.Locado)]
    [InlineData(StatusVeiculo.Manutencao)]
    public async Task AbrirAsync_VeiculoIndisponivel_LancaDomainException(StatusVeiculo status)
    {
        _clienteService.ObterAsync(2, Ct).Returns(Fabrica.Cliente(id: 2));
        _veiculoService.ObterAsync(3, Ct).Returns(Fabrica.Veiculo(id: 3, status: status));

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _servico.AbrirAsync(2, 3, Fabrica.Agora, Prevista, null, [], Ct));

        Assert.Equal("Veículo indisponível para locação.", ex.Message);
    }

    [Fact]
    public async Task AbrirAsync_EntradaAcimaDoSaldo_PropagaAMensagemDaEntidade()
    {
        _clienteService.ObterAsync(2, Ct).Returns(Fabrica.Cliente(id: 2));
        _veiculoService.ObterAsync(3, Ct).Returns(Fabrica.Veiculo(id: 3));
        PagamentoInformado[] entradas = [new(300.01m, "Pix")];

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _servico.AbrirAsync(2, 3, Fabrica.Agora, Prevista, null, entradas, Ct));

        Assert.Equal("O pagamento excede o saldo devedor de R$ 300,00.", ex.Message);
    }

    [Fact]
    public async Task AbrirAsync_ClienteNaoEncontrado_PropagaNaoEncontrado()
    {
        _clienteService.ObterAsync(2, Ct).ThrowsAsync(new NaoEncontradoException("Cliente", 2));

        var ex = await Assert.ThrowsAsync<NaoEncontradoException>(
            () => _servico.AbrirAsync(2, 3, Fabrica.Agora, Prevista, null, [], Ct));

        Assert.Equal("Cliente 2 não encontrado(a).", ex.Message);
    }

    [Fact]
    public async Task RegistrarEntradaAsync_LocacaoAberta_AdicionaPagamentoDeEntradaNaDataInformada()
    {
        var locacao = LocacaoAbertaNoRepositorio();
        var dataPagamento = Fabrica.Agora.AddDays(1);

        var resultado = await _servico.RegistrarEntradaAsync(5, new PagamentoInformado(120m, "Dinheiro"), dataPagamento, Ct);

        Assert.Same(locacao, resultado);
        var pagamento = Assert.Single(resultado.Pagamentos);
        Assert.Equal(TipoPagamento.Entrada, pagamento.Tipo);
        Assert.Equal(120m, pagamento.Valor);
        Assert.Equal(FormaPagamento.Dinheiro, pagamento.Forma);
        Assert.Equal(dataPagamento, pagamento.DataPagamento);
        Assert.Equal(120m, resultado.ValorPago);
    }

    [Fact]
    public async Task RegistrarEntradaAsync_LocacaoNaoExiste_LancaNaoEncontrado()
    {
        _locacoes.ObterComDetalhesAsync(5, Ct).Returns((Locacao?)null);

        var ex = await Assert.ThrowsAsync<NaoEncontradoException>(
            () => _servico.RegistrarEntradaAsync(5, new PagamentoInformado(120m, "Pix"), Fabrica.Agora, Ct));

        Assert.Equal("Locação 5 não encontrado(a).", ex.Message);
    }

    [Fact]
    public async Task ObterPreviaAsync_LocacaoAberta_DevolvePreviaDaEntidadeParaAData()
    {
        LocacaoAbertaNoRepositorio();

        var previa = await _servico.ObterPreviaAsync(5, Prevista, Ct);

        Assert.Equal(new PreviaDevolucao(3, 0, 300m, 0m, 300m, 0m, 300m), previa);
    }

    [Fact]
    public async Task FinalizarAsync_SaidasQueFechamOSaldo_FinalizaComPagamentosDeSaidaDatadosNaDevolucao()
    {
        var locacao = LocacaoAbertaNoRepositorio();
        PagamentoInformado[] saidas = [new(200m, "Pix"), new(100m, "Dinheiro")];

        var resultado = await _servico.FinalizarAsync(5, Prevista, 10_500, "Sem avarias", saidas, Ct);

        Assert.Same(locacao, resultado);
        Assert.Equal(StatusLocacao.Finalizada, resultado.Status);
        Assert.Equal(Prevista, resultado.DataDevolucao);
        Assert.Equal(10_500, resultado.KmDevolucao);
        Assert.Equal(2, resultado.Pagamentos.Count);
        Assert.All(resultado.Pagamentos, p =>
        {
            Assert.Equal(TipoPagamento.Saida, p.Tipo);
            Assert.Equal(Prevista, p.DataPagamento);
        });
        Assert.Equal(
            [FormaPagamento.Pix, FormaPagamento.Dinheiro],
            resultado.Pagamentos.Select(p => p.Forma));
        Assert.Equal(300m, resultado.ValorPago);
    }

    [Fact]
    public async Task FinalizarAsync_FormaDeSaidaInvalida_LancaMensagemDoConversor()
    {
        LocacaoAbertaNoRepositorio();
        PagamentoInformado[] saidas = [new(300m, "cheque")];

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _servico.FinalizarAsync(5, Prevista, 10_500, null, saidas, Ct));

        Assert.Equal(
            "Valor \"cheque\" inválido para forma de pagamento. Use: Dinheiro, CartaoCredito, CartaoDebito, Pix.",
            ex.Message);
    }

    [Fact]
    public async Task ProrrogarAsync_LocacaoNoPrazo_AtualizaDataPrevistaEValorPrevisto()
    {
        var locacao = LocacaoAbertaNoRepositorio();
        var novaData = Fabrica.Agora.AddDays(5);

        var resultado = await _servico.ProrrogarAsync(5, Fabrica.Agora, novaData, Ct);

        Assert.Same(locacao, resultado);
        Assert.Equal(novaData, resultado.DataPrevistaDevolucao);
        Assert.Equal(500m, resultado.ValorPrevisto);
    }

    [Fact]
    public async Task ProrrogarAsync_LocacaoVencida_PropagaAMensagemDaEntidade()
    {
        LocacaoAbertaNoRepositorio();

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _servico.ProrrogarAsync(5, Prevista.AddMinutes(1), Prevista.AddDays(2), Ct));

        Assert.Equal("Locação vencida não pode ser prorrogada.", ex.Message);
    }

    [Fact]
    public async Task CancelarAsync_LocacaoAberta_CancelaELiberaOVeiculo()
    {
        var locacao = LocacaoAbertaNoRepositorio();

        var resultado = await _servico.CancelarAsync(5, Ct);

        Assert.Same(locacao, resultado);
        Assert.Equal(StatusLocacao.Cancelada, resultado.Status);
        Assert.Equal(StatusVeiculo.Disponivel, resultado.Veiculo.Status);
    }

    [Fact]
    public async Task CancelarAsync_LocacaoJaCancelada_PropagaAMensagemDaEntidade()
    {
        LocacaoAbertaNoRepositorio().Cancelar();

        var ex = await Assert.ThrowsAsync<DomainException>(() => _servico.CancelarAsync(5, Ct));

        Assert.Equal("Só é possível cancelar locação aberta.", ex.Message);
    }
}
