using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Servicos;
using Locadora.Domain.Tests.Suporte;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Locadora.Domain.Tests.Servicos;

public sealed class VeiculoServiceTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private readonly IVeiculoRepository _veiculos = Substitute.For<IVeiculoRepository>();
    private readonly ICategoriaService _categoriaService = Substitute.For<ICategoriaService>();
    private readonly VeiculoService _servico;

    public VeiculoServiceTests() => _servico = new VeiculoService(_veiculos, _categoriaService);

    [Fact]
    public async Task ObterAsync_VeiculoExiste_DevolveAMesmaInstancia()
    {
        var veiculo = Fabrica.Veiculo(id: 5);
        _veiculos.ObterPorIdAsync(5, Ct).Returns(veiculo);

        var resultado = await _servico.ObterAsync(5, Ct);

        Assert.Same(veiculo, resultado);
    }

    [Fact]
    public async Task ObterAsync_VeiculoNaoExiste_LancaNaoEncontrado()
    {
        _veiculos.ObterPorIdAsync(5, Ct).Returns((Veiculo?)null);

        var ex = await Assert.ThrowsAsync<NaoEncontradoException>(() => _servico.ObterAsync(5, Ct));

        Assert.Equal("Veículo 5 não encontrado(a).", ex.Message);
    }

    [Fact]
    public async Task GarantirPlacaDisponivelAsync_PlacaLivre_NaoLanca()
    {
        _veiculos.ExistePlacaAsync("ABC1D23", Ct).Returns(false);

        await _servico.GarantirPlacaDisponivelAsync("ABC1D23", Ct);
    }

    [Fact]
    public async Task GarantirPlacaDisponivelAsync_PlacaJaExiste_LancaDomainException()
    {
        _veiculos.ExistePlacaAsync("ABC1D23", Ct).Returns(true);

        var ex = await Assert.ThrowsAsync<DomainException>(() => _servico.GarantirPlacaDisponivelAsync("ABC1D23", Ct));

        Assert.Equal("Já existe um veículo com esta placa.", ex.Message);
    }

    [Fact]
    public async Task ObterCategoriaAtivaAsync_CategoriaAtiva_DevolveAMesmaInstancia()
    {
        var categoria = Fabrica.Categoria(id: 3);
        _categoriaService.ObterAsync(3, Ct).Returns(categoria);

        var resultado = await _servico.ObterCategoriaAtivaAsync(3, Ct);

        Assert.Same(categoria, resultado);
    }

    [Fact]
    public async Task ObterCategoriaAtivaAsync_CategoriaInativa_LancaDomainException()
    {
        _categoriaService.ObterAsync(3, Ct).Returns(Fabrica.Categoria(id: 3, ativo: false));

        var ex = await Assert.ThrowsAsync<DomainException>(() => _servico.ObterCategoriaAtivaAsync(3, Ct));

        Assert.Equal("A categoria está inativa.", ex.Message);
    }

    [Fact]
    public async Task ObterCategoriaAtivaAsync_CategoriaInexistente_PropagaNaoEncontrado()
    {
        _categoriaService.ObterAsync(3, Ct).ThrowsAsync(new NaoEncontradoException("Categoria", 3));

        var ex = await Assert.ThrowsAsync<NaoEncontradoException>(() => _servico.ObterCategoriaAtivaAsync(3, Ct));

        Assert.Equal("Categoria 3 não encontrado(a).", ex.Message);
    }
}
