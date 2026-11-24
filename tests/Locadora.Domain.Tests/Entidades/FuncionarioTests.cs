using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Tests.Suporte;
using NSubstitute;

namespace Locadora.Domain.Tests.Entidades;

public sealed class FuncionarioTests
{
    [Fact]
    public void Construtor_DadosValidos_GuardaCamposENasceAtivo()
    {
        var funcionario = Fabrica.Funcionario(nome: "Ana Souza", email: "ana@locadora.com", senhaHash: "hash");

        Assert.Equal("Ana Souza", funcionario.Nome);
        Assert.Equal("ana@locadora.com", funcionario.Email);
        Assert.Equal("hash", funcionario.SenhaHash);
        Assert.True(funcionario.Ativo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_SenhaHashNulaVaziaOuEspacos_Lanca(string? senhaHash)
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Funcionario(senhaHash: senhaHash!));

        Assert.Equal("A senha é obrigatória.", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_NomeNuloVazioOuEspacos_Lanca(string? nome)
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Funcionario(nome: nome!));

        Assert.Equal("O nome do funcionário é obrigatório.", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_EmailNuloVazioOuEspacos_Lanca(string? email)
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Funcionario(email: email!));

        Assert.Equal("O e-mail do funcionário é obrigatório.", ex.Message);
    }

    [Fact]
    public void Construtor_NomeEEmailComEspacosEMaiusculas_NormalizaOsDois()
    {
        var funcionario = Fabrica.Funcionario(nome: "  Ana Souza  ", email: "  Ana@Locadora.COM  ");

        Assert.Equal("Ana Souza", funcionario.Nome);
        Assert.Equal("ana@locadora.com", funcionario.Email);
    }

    [Fact]
    public void Atualizar_DadosValidos_AlteraNomeEEmailNormalizado()
    {
        var funcionario = Fabrica.Funcionario();

        funcionario.Atualizar(" Bia Lima ", " BIA@Locadora.com ");

        Assert.Equal("Bia Lima", funcionario.Nome);
        Assert.Equal("bia@locadora.com", funcionario.Email);
    }

    [Fact]
    public void Atualizar_NomeVazio_Lanca()
    {
        var funcionario = Fabrica.Funcionario();

        var ex = Assert.Throws<DomainException>(() => funcionario.Atualizar(" ", "ana@locadora.com"));

        Assert.Equal("O nome do funcionário é obrigatório.", ex.Message);
    }

    [Fact]
    public void Atualizar_EmailVazio_LancaEMantemDados()
    {
        var funcionario = Fabrica.Funcionario(nome: "Ana Souza", email: "ana@locadora.com");

        var ex = Assert.Throws<DomainException>(() => funcionario.Atualizar("Outro Nome", ""));

        Assert.Equal("O e-mail do funcionário é obrigatório.", ex.Message);
        Assert.Equal("Ana Souza", funcionario.Nome);
        Assert.Equal("ana@locadora.com", funcionario.Email);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void VerificarSenha_DelegaAoHashComSenhaEHashGuardado(bool resultadoDoHash)
    {
        var funcionario = Fabrica.Funcionario(senhaHash: "hash-guardado");
        var hash = Substitute.For<IHashSenha>();
        hash.Verificar("senha-digitada", "hash-guardado").Returns(resultadoDoHash);

        var resultado = funcionario.VerificarSenha("senha-digitada", hash);

        Assert.Equal(resultadoDoHash, resultado);
        hash.Received(1).Verificar("senha-digitada", "hash-guardado");
    }

    [Fact]
    public void Inativar_ComOProprioId_LancaEMantemAtivo()
    {
        var funcionario = Fabrica.Funcionario(id: 5);

        var ex = Assert.Throws<DomainException>(() => funcionario.Inativar(idSolicitante: 5));

        Assert.Equal("Você não pode inativar o seu próprio usuário.", ex.Message);
        Assert.True(funcionario.Ativo);
    }

    [Fact]
    public void Inativar_ComOutroId_FicaInativo()
    {
        var funcionario = Fabrica.Funcionario(id: 5);

        funcionario.Inativar(idSolicitante: 6);

        Assert.False(funcionario.Ativo);
    }

    [Fact]
    public void Reativar_FuncionarioInativo_FicaAtivo()
    {
        var funcionario = Fabrica.Funcionario(ativo: false);

        funcionario.Reativar();

        Assert.True(funcionario.Ativo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345")]
    public void ValidarSenha_NulaVaziaOuAbaixoDoMinimo_Lanca(string? senha)
    {
        var ex = Assert.Throws<DomainException>(() => Funcionario.ValidarSenha(senha!));

        Assert.Equal("A senha deve ter no mínimo 6 caracteres.", ex.Message);
    }

    [Fact]
    public void ValidarSenha_ComTamanhoMinimo_NaoLanca()
    {
        var excecao = Record.Exception(
            () => Funcionario.ValidarSenha(new string('a', Funcionario.TamanhoMinimoSenha)));

        Assert.Null(excecao);
    }

    [Theory]
    [InlineData("  Ana@Locadora.COM  ", "ana@locadora.com")]
    [InlineData("ana@locadora.com", "ana@locadora.com")]
    public void NormalizarEmail_AplicaTrimEMinusculo(string email, string esperado)
    {
        Assert.Equal(esperado, Funcionario.NormalizarEmail(email));
    }
}
