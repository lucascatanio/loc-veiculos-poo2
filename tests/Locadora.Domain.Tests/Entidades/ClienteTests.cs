using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Tests.Suporte;

namespace Locadora.Domain.Tests.Entidades;

public sealed class ClienteTests
{
    // '٣' (U+0663) é dígito Unicode, mas não ASCII: não pode contar como dígito.
    private const char DigitoArabe = '٣';

    [Fact]
    public void Construtor_DadosValidos_GuardaCamposENasceAtivo()
    {
        var cliente = Fabrica.Cliente(
            nome: "Maria Silva", cpf: "52998224725", cnh: "12345678901",
            telefone: "11987654321", email: "maria@email.com");

        Assert.Equal("Maria Silva", cliente.Nome);
        Assert.Equal("52998224725", cliente.Cpf);
        Assert.Equal("12345678901", cliente.Cnh);
        Assert.Equal("11987654321", cliente.Telefone);
        Assert.Equal("maria@email.com", cliente.Email);
        Assert.True(cliente.Ativo);
    }

    [Fact]
    public void Construtor_CpfComMascara_GuardaSoDigitos()
    {
        var cliente = Fabrica.Cliente(cpf: "529.982.247-25");

        Assert.Equal("52998224725", cliente.Cpf);
    }

    [Theory]
    [InlineData("5299822472")]
    [InlineData("529982247251")]
    [InlineData("")]
    [InlineData("abc")]
    public void Construtor_CpfSemOnzeDigitos_Lanca(string cpf)
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Cliente(cpf: cpf));

        Assert.Equal("O CPF deve ter 11 dígitos.", ex.Message);
    }

    [Fact]
    public void Construtor_CpfNulo_Lanca()
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Cliente(cpf: null!));

        Assert.Equal("O CPF deve ter 11 dígitos.", ex.Message);
    }

    [Fact]
    public void Construtor_CpfComDigitoNaoAscii_NaoContaComoDigito()
    {
        var cpf = "5299822472" + DigitoArabe;

        var ex = Assert.Throws<DomainException>(() => Fabrica.Cliente(cpf: cpf));

        Assert.Equal("O CPF deve ter 11 dígitos.", ex.Message);
    }

    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("52998224725", "52998224725")]
    [InlineData(" 529 982 247 25 ", "52998224725")]
    [InlineData("", "")]
    [InlineData("abc", "")]
    public void NormalizarCpf_RemoveTudoQueNaoForDigito(string cpf, string esperado)
    {
        Assert.Equal(esperado, Cliente.NormalizarCpf(cpf));
    }

    [Fact]
    public void NormalizarCpf_Nulo_RetornaVazio()
    {
        Assert.Equal(string.Empty, Cliente.NormalizarCpf(null!));
    }

    [Fact]
    public void NormalizarCpf_DigitoNaoAscii_Descarta()
    {
        Assert.Equal("123", Cliente.NormalizarCpf("1" + DigitoArabe + "2" + DigitoArabe + "3"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_NomeNuloVazioOuEspacos_Lanca(string? nome)
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Cliente(nome: nome!));

        Assert.Equal("O nome do cliente é obrigatório.", ex.Message);
    }

    [Fact]
    public void Construtor_NomeComEspacosNasPontas_GuardaComTrim()
    {
        var cliente = Fabrica.Cliente(nome: "  Maria Silva  ");

        Assert.Equal("Maria Silva", cliente.Nome);
    }

    [Fact]
    public void Construtor_CnhComMascara_GuardaSoDigitos()
    {
        var cliente = Fabrica.Cliente(cnh: "123.456.789-01");

        Assert.Equal("12345678901", cliente.Cnh);
    }

    [Theory]
    [InlineData("1234567890")]
    [InlineData("123456789012")]
    [InlineData("")]
    public void Construtor_CnhSemOnzeDigitos_Lanca(string cnh)
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Cliente(cnh: cnh));

        Assert.Equal("A CNH deve ter 11 dígitos.", ex.Message);
    }

    [Fact]
    public void Construtor_CnhNula_Lanca()
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Cliente(cnh: null!));

        Assert.Equal("A CNH deve ter 11 dígitos.", ex.Message);
    }

    [Fact]
    public void Construtor_CnhComDigitoNaoAscii_NaoContaComoDigito()
    {
        var cnh = "1234567890" + DigitoArabe;

        var ex = Assert.Throws<DomainException>(() => Fabrica.Cliente(cnh: cnh));

        Assert.Equal("A CNH deve ter 11 dígitos.", ex.Message);
    }

    [Theory]
    [InlineData("1198765432")]
    [InlineData("11987654321")]
    public void Construtor_TelefoneComDezOuOnzeDigitos_Aceita(string telefone)
    {
        var cliente = Fabrica.Cliente(telefone: telefone);

        Assert.Equal(telefone, cliente.Telefone);
    }

    [Theory]
    [InlineData("119876543")]
    [InlineData("119876543210")]
    [InlineData("")]
    public void Construtor_TelefoneForaDaFaixa_Lanca(string telefone)
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Cliente(telefone: telefone));

        Assert.Equal("O telefone deve ter 10 ou 11 dígitos (com DDD).", ex.Message);
    }

    [Fact]
    public void Construtor_TelefoneNulo_Lanca()
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Cliente(telefone: null!));

        Assert.Equal("O telefone deve ter 10 ou 11 dígitos (com DDD).", ex.Message);
    }

    [Fact]
    public void Construtor_TelefoneComMascara_GuardaSoDigitos()
    {
        var cliente = Fabrica.Cliente(telefone: "(11) 98765-4321");

        Assert.Equal("11987654321", cliente.Telefone);
    }

    [Fact]
    public void Construtor_TelefoneComDigitoNaoAscii_NaoContaComoDigito()
    {
        var telefone = "119876543" + DigitoArabe;

        var ex = Assert.Throws<DomainException>(() => Fabrica.Cliente(telefone: telefone));

        Assert.Equal("O telefone deve ter 10 ou 11 dígitos (com DDD).", ex.Message);
    }

    [Fact]
    public void Construtor_EmailComEspacosEMaiusculas_GuardaComTrimEMinusculo()
    {
        var cliente = Fabrica.Cliente(email: "  Maria.Silva@Email.COM  ");

        Assert.Equal("maria.silva@email.com", cliente.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_EmailNuloVazioOuEspacos_GuardaNulo(string? email)
    {
        var cliente = Fabrica.Cliente(email: email);

        Assert.Null(cliente.Email);
    }

    [Fact]
    public void Atualizar_DadosValidos_AlteraCamposEMantemCpf()
    {
        var cliente = Fabrica.Cliente(cpf: "52998224725");

        cliente.Atualizar(" João Pereira ", "98765432100", "(21) 3456-7890", " Joao@Email.com ");

        Assert.Equal("João Pereira", cliente.Nome);
        Assert.Equal("98765432100", cliente.Cnh);
        Assert.Equal("2134567890", cliente.Telefone);
        Assert.Equal("joao@email.com", cliente.Email);
        Assert.Equal("52998224725", cliente.Cpf);
    }

    [Fact]
    public void Atualizar_EmailVazio_LimpaOEmail()
    {
        var cliente = Fabrica.Cliente(email: "maria@email.com");

        cliente.Atualizar("Maria Silva", "12345678901", "11987654321", "  ");

        Assert.Null(cliente.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Atualizar_NomeNuloVazioOuEspacos_Lanca(string? nome)
    {
        var cliente = Fabrica.Cliente();

        var ex = Assert.Throws<DomainException>(
            () => cliente.Atualizar(nome!, "12345678901", "11987654321", null));

        Assert.Equal("O nome do cliente é obrigatório.", ex.Message);
    }

    [Fact]
    public void Atualizar_CnhInvalida_LancaEMantemDados()
    {
        var cliente = Fabrica.Cliente(nome: "Maria Silva", cnh: "12345678901");

        var ex = Assert.Throws<DomainException>(
            () => cliente.Atualizar("Outro Nome", "123", "11987654321", null));

        Assert.Equal("A CNH deve ter 11 dígitos.", ex.Message);
        Assert.Equal("Maria Silva", cliente.Nome);
        Assert.Equal("12345678901", cliente.Cnh);
    }

    [Fact]
    public void Atualizar_TelefoneInvalido_Lanca()
    {
        var cliente = Fabrica.Cliente();

        var ex = Assert.Throws<DomainException>(
            () => cliente.Atualizar("Maria Silva", "12345678901", "123", null));

        Assert.Equal("O telefone deve ter 10 ou 11 dígitos (com DDD).", ex.Message);
    }

    [Fact]
    public void Inativar_ClienteAtivo_FicaInativo()
    {
        var cliente = Fabrica.Cliente();

        cliente.Inativar();

        Assert.False(cliente.Ativo);
    }

    [Fact]
    public void Reativar_ClienteInativo_FicaAtivo()
    {
        var cliente = Fabrica.Cliente(ativo: false);

        cliente.Reativar();

        Assert.True(cliente.Ativo);
    }
}
