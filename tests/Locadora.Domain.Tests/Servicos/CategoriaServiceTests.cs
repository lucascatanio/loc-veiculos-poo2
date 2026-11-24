using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Servicos;
using Locadora.Domain.Tests.Suporte;
using NSubstitute;

namespace Locadora.Domain.Tests.Servicos;

public sealed class CategoriaServiceTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private readonly ICategoriaRepository _categorias = Substitute.For<ICategoriaRepository>();
    private readonly IVeiculoRepository _veiculos = Substitute.For<IVeiculoRepository>();
    private readonly CategoriaService _servico;

    public CategoriaServiceTests() => _servico = new CategoriaService(_categorias, _veiculos);

    [Fact]
    public async Task ObterAsync_CategoriaExiste_DevolveAMesmaInstanciaERepassaOToken()
    {
        var categoria = Fabrica.Categoria(id: 5);
        _categorias.ObterPorIdAsync(5, Ct).Returns(categoria);

        var resultado = await _servico.ObterAsync(5, Ct);

        Assert.Same(categoria, resultado);
        await _categorias.Received(1).ObterPorIdAsync(5, Ct);
    }

    [Fact]
    public async Task ObterAsync_CategoriaNaoExiste_LancaNaoEncontrado()
    {
        _categorias.ObterPorIdAsync(5, Ct).Returns((Categoria?)null);

        var ex = await Assert.ThrowsAsync<NaoEncontradoException>(() => _servico.ObterAsync(5, Ct));

        Assert.Equal("Categoria 5 não encontrado(a).", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(7)]
    public async Task GarantirNomeDisponivelAsync_NomeLivre_RepassaNomeSemEspacosEIgnorarId(int? ignorarId)
    {
        _categorias.ExisteNomeAsync(Arg.Any<string>(), Arg.Any<int?>(), Ct).Returns(false);

        await _servico.GarantirNomeDisponivelAsync("  SUV  ", ignorarId, Ct);

        await _categorias.Received(1).ExisteNomeAsync("SUV", ignorarId, Ct);
    }

    [Fact]
    public async Task GarantirNomeDisponivelAsync_NomeJaExiste_LancaDomainException()
    {
        _categorias.ExisteNomeAsync("SUV", null, Ct).Returns(true);

        var ex = await Assert.ThrowsAsync<DomainException>(() => _servico.GarantirNomeDisponivelAsync("SUV", null, Ct));

        Assert.Equal("Já existe uma categoria com este nome.", ex.Message);
    }

    [Fact]
    public async Task GarantirQuePodeInativarAsync_SemVeiculosAtivos_NaoLanca()
    {
        _veiculos.ContarAtivosNaCategoriaAsync(3, Ct).Returns(0);

        await _servico.GarantirQuePodeInativarAsync(3, Ct);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public async Task GarantirQuePodeInativarAsync_ComVeiculosAtivos_LancaComAQuantidade(int quantidade)
    {
        _veiculos.ContarAtivosNaCategoriaAsync(3, Ct).Returns(quantidade);

        var ex = await Assert.ThrowsAsync<DomainException>(() => _servico.GarantirQuePodeInativarAsync(3, Ct));

        Assert.Equal($"Existem {quantidade} veículo(s) ativo(s) nesta categoria.", ex.Message);
    }
}
