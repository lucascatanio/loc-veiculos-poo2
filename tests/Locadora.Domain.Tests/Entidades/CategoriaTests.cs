using Locadora.Domain.Excecoes;
using Locadora.Domain.Tests.Suporte;

namespace Locadora.Domain.Tests.Entidades;

public sealed class CategoriaTests
{
    public static TheoryData<decimal> DiariasNaoPositivas => new() { 0m, -0.01m, -100m };

    public static TheoryData<decimal> DiariasComMaisDeDuasCasas => new() { 100.001m, 0.005m, 99.999m };

    public static TheoryData<decimal> DiariasValidas => new() { 0.01m, 1m, 100m, 100.5m, 100.55m };

    [Fact]
    public void Construtor_DadosValidos_NasceAtiva()
    {
        var categoria = Fabrica.Categoria(nome: "SUV", valorDiaria: 150m);

        Assert.Equal("SUV", categoria.Nome);
        Assert.Equal(150m, categoria.ValorDiaria);
        Assert.True(categoria.Ativo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_NomeNuloVazioOuEspacos_Lanca(string? nome)
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Categoria(nome: nome!));

        Assert.Equal("O nome da categoria é obrigatório.", ex.Message);
    }

    [Fact]
    public void Construtor_NomeComEspacosNasPontas_GuardaComTrim()
    {
        var categoria = Fabrica.Categoria(nome: "  Econômico  ");

        Assert.Equal("Econômico", categoria.Nome);
    }

    [Theory]
    [MemberData(nameof(DiariasNaoPositivas))]
    public void Construtor_DiariaZeroOuNegativa_Lanca(decimal valorDiaria)
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Categoria(valorDiaria: valorDiaria));

        Assert.Equal("O valor da diária deve ser maior que zero.", ex.Message);
    }

    [Theory]
    [MemberData(nameof(DiariasComMaisDeDuasCasas))]
    public void Construtor_DiariaComMaisDeDuasCasas_Lanca(decimal valorDiaria)
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Categoria(valorDiaria: valorDiaria));

        Assert.Equal("O valor da diária deve ter no máximo 2 casas decimais.", ex.Message);
    }

    [Theory]
    [MemberData(nameof(DiariasValidas))]
    public void Construtor_DiariaPositivaComAteDuasCasas_Aceita(decimal valorDiaria)
    {
        var categoria = Fabrica.Categoria(valorDiaria: valorDiaria);

        Assert.Equal(valorDiaria, categoria.ValorDiaria);
    }

    [Fact]
    public void Atualizar_DadosValidos_AlteraNomeEDiaria()
    {
        var categoria = Fabrica.Categoria(nome: "SUV", valorDiaria: 100m);

        categoria.Atualizar("  Premium ", 250.50m);

        Assert.Equal("Premium", categoria.Nome);
        Assert.Equal(250.50m, categoria.ValorDiaria);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Atualizar_NomeNuloVazioOuEspacos_LancaEMantemDados(string? nome)
    {
        var categoria = Fabrica.Categoria(nome: "SUV", valorDiaria: 100m);

        var ex = Assert.Throws<DomainException>(() => categoria.Atualizar(nome!, 200m));

        Assert.Equal("O nome da categoria é obrigatório.", ex.Message);
        Assert.Equal("SUV", categoria.Nome);
        Assert.Equal(100m, categoria.ValorDiaria);
    }

    [Theory]
    [MemberData(nameof(DiariasNaoPositivas))]
    public void Atualizar_DiariaZeroOuNegativa_Lanca(decimal valorDiaria)
    {
        var categoria = Fabrica.Categoria();

        var ex = Assert.Throws<DomainException>(() => categoria.Atualizar("SUV", valorDiaria));

        Assert.Equal("O valor da diária deve ser maior que zero.", ex.Message);
    }

    [Theory]
    [MemberData(nameof(DiariasComMaisDeDuasCasas))]
    public void Atualizar_DiariaComMaisDeDuasCasas_Lanca(decimal valorDiaria)
    {
        var categoria = Fabrica.Categoria();

        var ex = Assert.Throws<DomainException>(() => categoria.Atualizar("SUV", valorDiaria));

        Assert.Equal("O valor da diária deve ter no máximo 2 casas decimais.", ex.Message);
    }

    [Fact]
    public void Inativar_CategoriaAtiva_FicaInativa()
    {
        var categoria = Fabrica.Categoria();

        categoria.Inativar();

        Assert.False(categoria.Ativo);
    }

    [Fact]
    public void Reativar_CategoriaInativa_FicaAtiva()
    {
        var categoria = Fabrica.Categoria(ativo: false);

        categoria.Reativar();

        Assert.True(categoria.Ativo);
    }
}
