using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Servicos;
using Locadora.Domain.Tests.Apoio;
using Locadora.Domain.Valores;
using NSubstitute;

namespace Locadora.Domain.Tests.R1_DevolucaoComMulta;

[Trait(Tecnica.Requisito, Tecnica.Devolucao)]
[Trait(Tecnica.Nome, Tecnica.TabelaDecisao)]
public class LocacaoServiceDevolucaoTests
{
    private const int IdLocacao = 42;
    private const int KmValido = Cenario.KmInicial + 500;

    private readonly ILocacaoRepository _locacoes = Substitute.For<ILocacaoRepository>();
    private readonly LocacaoService _servico;

    public LocacaoServiceDevolucaoTests() =>
        _servico = new LocacaoService(_locacoes, Substitute.For<IClienteService>(), Substitute.For<IVeiculoService>());

    private Locacao LocacaoCadastrada()
    {
        var locacao = Cenario.LocacaoAberta();
        _locacoes.ObterComDetalhesAsync(IdLocacao, Arg.Any<CancellationToken>()).Returns(locacao);
        return locacao;
    }

    [Fact]
    public async Task FinalizarAsync_LocacaoInexistente_LancaNaoEncontrado()
    {
        _locacoes.ObterComDetalhesAsync(IdLocacao, Arg.Any<CancellationToken>()).Returns((Locacao?)null);

        await Assert.ThrowsAsync<NaoEncontradoException>(() =>
            _servico.FinalizarAsync(IdLocacao, Cenario.Prevista, KmValido, null, [], CancellationToken.None));
    }

    [Fact]
    public async Task FinalizarAsync_SaidasInformadas_ViramPagamentosDeSaidaNaDataDaDevolucao()
    {
        LocacaoCadastrada();
        var devolucao = Cenario.Prevista.AddMinutes(30);
        PagamentoInformado[] saidas = [new(200m, "pix"), new(100m, "Dinheiro")];

        var locacao = await _servico.FinalizarAsync(IdLocacao, devolucao, KmValido, null, saidas, CancellationToken.None);

        Assert.Equal(StatusLocacao.Finalizada, locacao.Status);
        Assert.All(locacao.Pagamentos, p =>
        {
            Assert.Equal(TipoPagamento.Saida, p.Tipo);
            Assert.Equal(devolucao, p.DataPagamento);
        });
        Assert.Equal(new[] { FormaPagamento.Pix, FormaPagamento.Dinheiro },locacao.Pagamentos.Select(p => p.Forma));
    }

    [Theory]
    [InlineData("Boleto")]
    [InlineData("")]
    [InlineData("7")]
    public async Task FinalizarAsync_FormaDePagamentoInexistente_LancaExcecaoSemFinalizar(string forma)
    {
        var cadastrada = LocacaoCadastrada();

        await Assert.ThrowsAsync<DomainException>(() => _servico.FinalizarAsync(
            IdLocacao, Cenario.Prevista, KmValido, null, [new(300m, forma)], CancellationToken.None));
        Assert.Equal(StatusLocacao.Aberta, cadastrada.Status);
    }

    // A mensagem de erro do ConversorEnum diz "Use: Dinheiro, CartaoCredito, CartaoDebito, Pix":
    // número ou combinação de nomes não deveriam ser aceitos como forma de pagamento.
    [Theory]
    [InlineData("0")]
    [InlineData("3")]
    [InlineData("CartaoCredito,CartaoDebito")]
    public async Task FinalizarAsync_FormaInformadaPorNumeroOuCombinacao_LancaExcecao(string forma)
    {
        LocacaoCadastrada();

        await Assert.ThrowsAsync<DomainException>(() => _servico.FinalizarAsync(
            IdLocacao, Cenario.Prevista, KmValido, null, [new(300m, forma)], CancellationToken.None));
    }

    [Fact]
    public async Task ObterPreviaAsync_LocacaoExistente_RetornaPreviaDaEntidade()
    {
        LocacaoCadastrada();

        var previa = await _servico.ObterPreviaAsync(IdLocacao, Cenario.Prevista.AddHours(2), CancellationToken.None);

        Assert.Equal(1, previa.DiasAtraso);
        Assert.Equal(420m, previa.Saldo);
    }
}
