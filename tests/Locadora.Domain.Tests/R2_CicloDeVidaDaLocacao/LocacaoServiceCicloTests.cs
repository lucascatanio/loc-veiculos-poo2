using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Servicos;
using Locadora.Domain.Tests.Apoio;
using Locadora.Domain.Valores;
using NSubstitute;

namespace Locadora.Domain.Tests.R2_CicloDeVidaDaLocacao;

[Trait(Tecnica.Requisito, Tecnica.CicloLocacao)]
[Trait(Tecnica.Nome, Tecnica.TransicaoEstados)]
public class LocacaoServiceCicloTests
{
    private const int IdCliente = 7;
    private const int IdVeiculo = 9;
    private const int IdLocacao = 42;

    private readonly ILocacaoRepository _locacoes = Substitute.For<ILocacaoRepository>();
    private readonly IClienteService _clientes = Substitute.For<IClienteService>();
    private readonly IVeiculoService _veiculos = Substitute.For<IVeiculoService>();
    private readonly LocacaoService _servico;

    public LocacaoServiceCicloTests() => _servico = new LocacaoService(_locacoes, _clientes, _veiculos);

    private Cliente ClienteCadastrado()
    {
        var cliente = Cenario.Cliente();
        _clientes.ObterAsync(IdCliente, Arg.Any<CancellationToken>()).Returns(cliente);
        return cliente;
    }

    private Veiculo VeiculoCadastrado()
    {
        var veiculo = Cenario.Veiculo();
        _veiculos.ObterAsync(IdVeiculo, Arg.Any<CancellationToken>()).Returns(veiculo);
        return veiculo;
    }

    private Task<Locacao> Abrir(params PagamentoInformado[] entradas) =>
        _servico.AbrirAsync(IdCliente, IdVeiculo, Cenario.Retirada, Cenario.Prevista, null, entradas, CancellationToken.None);

    [Fact]
    public async Task AbrirAsync_ClienteEVeiculoAptos_AbreLocacaoComEntradasNaDataDaRetirada()
    {
        ClienteCadastrado();
        var veiculo = VeiculoCadastrado();

        var locacao = await Abrir(new(100m, "Pix"), new(50m, "dinheiro"));

        Assert.Equal(StatusLocacao.Aberta, locacao.Status);
        Assert.Equal(StatusVeiculo.Locado, veiculo.Status);
        Assert.Equal(150m, locacao.ValorPago);
        Assert.All(locacao.Pagamentos, p =>
        {
            Assert.Equal(TipoPagamento.Entrada, p.Tipo);
            Assert.Equal(Cenario.Retirada, p.DataPagamento);
        });
    }

    [Fact]
    public async Task AbrirAsync_ClienteInativo_LancaExcecaoSemConsultarVeiculo()
    {
        ClienteCadastrado().Inativar();
        var veiculo = VeiculoCadastrado();

        await Assert.ThrowsAsync<DomainException>(() => Abrir());

        await _veiculos.DidNotReceive().ObterAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        Assert.Equal(StatusVeiculo.Disponivel, veiculo.Status);
    }

    [Fact]
    public async Task AbrirAsync_VeiculoEmManutencao_LancaExcecao()
    {
        ClienteCadastrado();
        VeiculoCadastrado().EnviarParaManutencao();

        await Assert.ThrowsAsync<DomainException>(() => Abrir());
    }

    [Fact]
    public async Task AbrirAsync_EntradasPassamDoPrevisto_LancaExcecao()
    {
        ClienteCadastrado();
        VeiculoCadastrado();

        await Assert.ThrowsAsync<DomainException>(() => Abrir(new(200m, "Pix"), new(200m, "Pix")));
    }

    [Fact]
    public async Task CancelarAsync_LocacaoInexistente_LancaNaoEncontrado()
    {
        _locacoes.ObterComDetalhesAsync(IdLocacao, Arg.Any<CancellationToken>()).Returns((Locacao?)null);

        await Assert.ThrowsAsync<NaoEncontradoException>(() => _servico.CancelarAsync(IdLocacao, CancellationToken.None));
    }

    [Fact]
    public async Task CancelarAsync_LocacaoAberta_CancelaELiberaVeiculo()
    {
        var cadastrada = Cenario.LocacaoAberta();
        _locacoes.ObterComDetalhesAsync(IdLocacao, Arg.Any<CancellationToken>()).Returns(cadastrada);

        var locacao = await _servico.CancelarAsync(IdLocacao, CancellationToken.None);

        Assert.Equal(StatusLocacao.Cancelada, locacao.Status);
        Assert.Equal(StatusVeiculo.Disponivel, locacao.Veiculo.Status);
    }

    [Fact]
    public async Task ProrrogarAsync_LocacaoCancelada_LancaExcecao()
    {
        _locacoes.ObterComDetalhesAsync(IdLocacao, Arg.Any<CancellationToken>()).Returns(Cenario.LocacaoCancelada());

        await Assert.ThrowsAsync<DomainException>(() => _servico.ProrrogarAsync(
            IdLocacao, Cenario.Retirada, Cenario.Prevista.AddDays(1), CancellationToken.None));
    }

    [Theory]
    [InlineData(null, StatusLocacao.Aberta)]
    [InlineData("", StatusLocacao.Aberta)]
    [InlineData("   ", StatusLocacao.Aberta)]
    [InlineData("finalizada", StatusLocacao.Finalizada)]
    [InlineData("CANCELADA", StatusLocacao.Cancelada)]
    public async Task ListarAsync_FiltroDeStatus_ConsultaOEstadoCorreto(string? status, StatusLocacao esperado)
    {
        await _servico.ListarAsync(status, CancellationToken.None);

        await _locacoes.Received(1).ListarAsync(esperado, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Encerrada")]
    [InlineData("9")]
    [InlineData("1")]
    public async Task ListarAsync_StatusQueNaoEhNomeDeEstado_LancaExcecao(string status)
    {
        await Assert.ThrowsAsync<DomainException>(() => _servico.ListarAsync(status, CancellationToken.None));
    }
}
