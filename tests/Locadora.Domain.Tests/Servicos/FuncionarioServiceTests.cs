using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Servicos;
using Locadora.Domain.Tests.Suporte;
using NSubstitute;

namespace Locadora.Domain.Tests.Servicos;

public sealed class FuncionarioServiceTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private readonly IFuncionarioRepository _funcionarios = Substitute.For<IFuncionarioRepository>();
    private readonly IHashSenha _hash = Substitute.For<IHashSenha>();
    private readonly FuncionarioService _servico;

    public FuncionarioServiceTests() => _servico = new FuncionarioService(_funcionarios, _hash);

    [Fact]
    public async Task ObterAsync_FuncionarioExiste_DevolveAMesmaInstancia()
    {
        var funcionario = Fabrica.Funcionario(id: 5);
        _funcionarios.ObterPorIdAsync(5, Ct).Returns(funcionario);

        var resultado = await _servico.ObterAsync(5, Ct);

        Assert.Same(funcionario, resultado);
    }

    [Fact]
    public async Task ObterAsync_FuncionarioNaoExiste_LancaNaoEncontrado()
    {
        _funcionarios.ObterPorIdAsync(5, Ct).Returns((Funcionario?)null);

        var ex = await Assert.ThrowsAsync<NaoEncontradoException>(() => _servico.ObterAsync(5, Ct));

        Assert.Equal("Funcionário 5 não encontrado(a).", ex.Message);
    }

    [Fact]
    public async Task AutenticarAsync_EmailComCaixaEEspacos_ConsultaComEmailNormalizadoERepassaOToken()
    {
        _funcionarios.ObterPorEmailAsync(Arg.Any<string>(), Ct).Returns((Funcionario?)null);

        await _servico.AutenticarAsync("  Ana@Locadora.COM ", "segredo", Ct);

        await _funcionarios.Received(1).ObterPorEmailAsync("ana@locadora.com", Ct);
    }

    [Fact]
    public async Task AutenticarAsync_EmailNaoCadastrado_DevolveNulo()
    {
        _funcionarios.ObterPorEmailAsync("ana@locadora.com", Ct).Returns((Funcionario?)null);

        var resultado = await _servico.AutenticarAsync("ana@locadora.com", "segredo", Ct);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task AutenticarAsync_FuncionarioInativo_DevolveNuloSemVerificarASenha()
    {
        var inativo = Fabrica.Funcionario(ativo: false);
        _funcionarios.ObterPorEmailAsync("ana@locadora.com", Ct).Returns(inativo);
        _hash.Verificar(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var resultado = await _servico.AutenticarAsync("ana@locadora.com", "segredo", Ct);

        Assert.Null(resultado);
        _hash.DidNotReceiveWithAnyArgs().Verificar(default!, default!);
    }

    [Fact]
    public async Task AutenticarAsync_SenhaIncorreta_DevolveNulo()
    {
        var funcionario = Fabrica.Funcionario();
        _funcionarios.ObterPorEmailAsync("ana@locadora.com", Ct).Returns(funcionario);
        _hash.Verificar("errada", funcionario.SenhaHash).Returns(false);

        var resultado = await _servico.AutenticarAsync("ana@locadora.com", "errada", Ct);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task AutenticarAsync_SenhaCorreta_DevolveOFuncionarioVerificandoSenhaContraOHash()
    {
        var funcionario = Fabrica.Funcionario(senhaHash: "hash-armazenado");
        _funcionarios.ObterPorEmailAsync("ana@locadora.com", Ct).Returns(funcionario);
        _hash.Verificar("segredo", "hash-armazenado").Returns(true);

        var resultado = await _servico.AutenticarAsync("ana@locadora.com", "segredo", Ct);

        Assert.Same(funcionario, resultado);
        _hash.Received(1).Verificar("segredo", "hash-armazenado");
    }

    [Fact]
    public async Task EstaAtivoAsync_FuncionarioAtivo_DevolveVerdadeiro()
    {
        _funcionarios.ObterPorIdAsync(5, Ct).Returns(Fabrica.Funcionario(id: 5));

        Assert.True(await _servico.EstaAtivoAsync(5, Ct));
    }

    [Fact]
    public async Task EstaAtivoAsync_FuncionarioInativo_DevolveFalso()
    {
        _funcionarios.ObterPorIdAsync(5, Ct).Returns(Fabrica.Funcionario(id: 5, ativo: false));

        Assert.False(await _servico.EstaAtivoAsync(5, Ct));
    }

    [Fact]
    public async Task EstaAtivoAsync_FuncionarioInexistente_DevolveFalso()
    {
        _funcionarios.ObterPorIdAsync(5, Ct).Returns((Funcionario?)null);

        Assert.False(await _servico.EstaAtivoAsync(5, Ct));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(7)]
    public async Task GarantirEmailDisponivelAsync_EmailLivre_RepassaEmailNormalizadoEIgnorarId(int? ignorarId)
    {
        _funcionarios.ExisteEmailAsync(Arg.Any<string>(), Arg.Any<int?>(), Ct).Returns(false);

        await _servico.GarantirEmailDisponivelAsync("  Ana@Locadora.COM ", ignorarId, Ct);

        await _funcionarios.Received(1).ExisteEmailAsync("ana@locadora.com", ignorarId, Ct);
    }

    [Fact]
    public async Task GarantirEmailDisponivelAsync_EmailJaExiste_LancaDomainException()
    {
        _funcionarios.ExisteEmailAsync("ana@locadora.com", null, Ct).Returns(true);

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _servico.GarantirEmailDisponivelAsync("ana@locadora.com", null, Ct));

        Assert.Equal("Já existe um funcionário com este e-mail.", ex.Message);
    }
}
