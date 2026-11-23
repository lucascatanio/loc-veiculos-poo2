using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Servicos;
using Locadora.Domain.Tests.Apoio;
using NSubstitute;

namespace Locadora.Domain.Tests.R3_StatusDoVeiculo;

[Trait(Tecnica.Requisito, Tecnica.StatusVeiculo)]
[Trait(Tecnica.Nome, Tecnica.TabelaDecisao)]
public class VeiculoServiceTests
{
    private const string PlacaCadastrada = "ABC1D23";

    private readonly IVeiculoRepository _veiculos = Substitute.For<IVeiculoRepository>();
    private readonly ICategoriaService _categorias = Substitute.For<ICategoriaService>();
    private readonly VeiculoService _servico;

    public VeiculoServiceTests()
    {
        _servico = new VeiculoService(_veiculos, _categorias);
        _veiculos.ExistePlacaAsync(PlacaCadastrada, Arg.Any<CancellationToken>()).Returns(true);
    }

    [Fact]
    public async Task ObterAsync_VeiculoInexistente_LancaNaoEncontrado()
    {
        _veiculos.ObterPorIdAsync(9, Arg.Any<CancellationToken>()).Returns((Veiculo?)null);

        await Assert.ThrowsAsync<NaoEncontradoException>(() => _servico.ObterAsync(9, CancellationToken.None));
    }

    [Fact]
    public async Task ObterCategoriaAtivaAsync_CategoriaAtiva_RetornaCategoria()
    {
        var categoria = Cenario.Categoria();
        _categorias.ObterAsync(1, Arg.Any<CancellationToken>()).Returns(categoria);

        Assert.Same(categoria, await _servico.ObterCategoriaAtivaAsync(1, CancellationToken.None));
    }

    [Fact]
    public async Task ObterCategoriaAtivaAsync_CategoriaInativa_LancaExcecao()
    {
        var categoria = Cenario.Categoria();
        categoria.Inativar();
        _categorias.ObterAsync(1, Arg.Any<CancellationToken>()).Returns(categoria);

        await Assert.ThrowsAsync<DomainException>(() => _servico.ObterCategoriaAtivaAsync(1, CancellationToken.None));
    }

    [Fact]
    public async Task GarantirPlacaDisponivelAsync_PlacaLivre_NaoLancaExcecao()
    {
        await _servico.GarantirPlacaDisponivelAsync("XYZ9876", CancellationToken.None);
    }

    [Fact]
    public async Task GarantirPlacaDisponivelAsync_PlacaJaCadastrada_LancaExcecao()
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            _servico.GarantirPlacaDisponivelAsync(PlacaCadastrada, CancellationToken.None));
    }

    // CPF (ClienteService) e e-mail (FuncionarioService) são normalizados no próprio serviço antes da
    // consulta de duplicidade; a placa não. Hoje o VeiculoAppService passa veiculo.Placa já normalizada,
    // então o defeito é latente: aparece para qualquer outro chamador do serviço.
    [Theory]
    [InlineData("abc1d23")]
    [InlineData("ABC-1D23")]
    [InlineData(" abc 1d23 ")]
    public async Task GarantirPlacaDisponivelAsync_PlacaCadastradaEmOutroFormato_LancaExcecao(string placa)
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            _servico.GarantirPlacaDisponivelAsync(placa, CancellationToken.None));
    }
}
