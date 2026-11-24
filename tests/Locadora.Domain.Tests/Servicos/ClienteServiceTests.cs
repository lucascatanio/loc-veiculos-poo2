using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Servicos;
using Locadora.Domain.Tests.Suporte;
using NSubstitute;

namespace Locadora.Domain.Tests.Servicos;

public sealed class ClienteServiceTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private readonly IClienteRepository _clientes = Substitute.For<IClienteRepository>();
    private readonly ILocacaoRepository _locacoes = Substitute.For<ILocacaoRepository>();
    private readonly ClienteService _servico;

    public ClienteServiceTests() => _servico = new ClienteService(_clientes, _locacoes);

    [Fact]
    public async Task ObterAsync_ClienteExiste_DevolveAMesmaInstancia()
    {
        var cliente = Fabrica.Cliente(id: 5);
        _clientes.ObterPorIdAsync(5, Ct).Returns(cliente);

        var resultado = await _servico.ObterAsync(5, Ct);

        Assert.Same(cliente, resultado);
    }

    [Fact]
    public async Task ObterAsync_ClienteNaoExiste_LancaNaoEncontrado()
    {
        _clientes.ObterPorIdAsync(5, Ct).Returns((Cliente?)null);

        var ex = await Assert.ThrowsAsync<NaoEncontradoException>(() => _servico.ObterAsync(5, Ct));

        Assert.Equal("Cliente 5 não encontrado(a).", ex.Message);
    }

    [Fact]
    public async Task GarantirCpfDisponivelAsync_CpfComMascara_ConsultaSoComOsDigitos()
    {
        _clientes.ExisteCpfAsync(Arg.Any<string>(), Ct).Returns(false);

        await _servico.GarantirCpfDisponivelAsync("529.982.247-25", Ct);

        await _clientes.Received(1).ExisteCpfAsync("52998224725", Ct);
    }

    [Fact]
    public async Task GarantirCpfDisponivelAsync_CpfJaExiste_LancaDomainException()
    {
        _clientes.ExisteCpfAsync("52998224725", Ct).Returns(true);

        var ex = await Assert.ThrowsAsync<DomainException>(() => _servico.GarantirCpfDisponivelAsync("529.982.247-25", Ct));

        Assert.Equal("Já existe um cliente com este CPF.", ex.Message);
    }

    [Fact]
    public async Task GarantirQuePodeInativarAsync_ComLocacaoAberta_LancaDomainException()
    {
        _locacoes.ExisteAbertaParaClienteAsync(4, Ct).Returns(true);

        var ex = await Assert.ThrowsAsync<DomainException>(() => _servico.GarantirQuePodeInativarAsync(4, Ct));

        Assert.Equal("O cliente tem locação aberta.", ex.Message);
    }

    [Fact]
    public async Task GarantirQuePodeInativarAsync_SemLocacaoAberta_NaoLanca()
    {
        _locacoes.ExisteAbertaParaClienteAsync(4, Ct).Returns(false);

        await _servico.GarantirQuePodeInativarAsync(4, Ct);
    }
}
