using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Tests.Suporte;

namespace Locadora.Domain.Tests.Entidades;

public sealed class VeiculoTests
{
    [Fact]
    public void Construtor_DadosValidos_GuardaCamposDisponivelEAtivo()
    {
        var categoria = Fabrica.Categoria(id: 3);

        var veiculo = Fabrica.Veiculo(
            placa: "ABC1D23", marca: "Fiat", modelo: "Argo", ano: 2022, cor: "Branco",
            kmAtual: 15_000, categoria: categoria);

        Assert.Equal("ABC1D23", veiculo.Placa);
        Assert.Equal("Fiat", veiculo.Marca);
        Assert.Equal("Argo", veiculo.Modelo);
        Assert.Equal(2022, veiculo.Ano);
        Assert.Equal("Branco", veiculo.Cor);
        Assert.Equal(15_000, veiculo.KmAtual);
        Assert.Equal(StatusVeiculo.Disponivel, veiculo.Status);
        Assert.True(veiculo.Ativo);
        Assert.Same(categoria, veiculo.Categoria);
    }

    [Fact]
    public void Construtor_Categoria_CategoriaIdVemDaCategoria()
    {
        var veiculo = Fabrica.Veiculo(categoria: Fabrica.Categoria(id: 7));

        Assert.Equal(7, veiculo.CategoriaId);
    }

    [Theory]
    [InlineData("ABC1234")]
    [InlineData("ABC1D23")]
    public void Construtor_PlacaAntigaOuMercosul_Aceita(string placa)
    {
        var veiculo = Fabrica.Veiculo(placa: placa);

        Assert.Equal(placa, veiculo.Placa);
    }

    [Theory]
    [InlineData("abc-1234", "ABC1234")]
    [InlineData("ABC 1D23", "ABC1D23")]
    [InlineData("abc1d23", "ABC1D23")]
    [InlineData(" abc - 1234 ", "ABC1234")]
    public void Construtor_PlacaComMinusculaHifenOuEspaco_Normaliza(string placa, string esperada)
    {
        var veiculo = Fabrica.Veiculo(placa: placa);

        Assert.Equal(esperada, veiculo.Placa);
    }

    [Theory]
    [InlineData("AB12345")]
    [InlineData("ABCD123")]
    [InlineData("ABC12D3")]
    [InlineData("ABC123")]
    [InlineData("ABC12345")]
    [InlineData("1234567")]
    [InlineData("ABC1D2E")]
    [InlineData("ABC1#23")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("-")]
    public void Construtor_PlacaInvalida_Lanca(string placa)
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Veiculo(placa: placa));

        Assert.Equal("Placa inválida.", ex.Message);
    }

    [Fact]
    public void Construtor_PlacaNula_Lanca()
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Veiculo(placa: null!));

        Assert.Equal("Placa inválida.", ex.Message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10_000)]
    public void Construtor_KmNegativo_Lanca(int km)
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Veiculo(kmAtual: km));

        Assert.Equal("O km atual não pode ser negativo.", ex.Message);
    }

    [Fact]
    public void Construtor_KmZero_Aceita()
    {
        var veiculo = Fabrica.Veiculo(kmAtual: 0);

        Assert.Equal(0, veiculo.KmAtual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_MarcaNulaVaziaOuEspacos_Lanca(string? marca)
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Veiculo(marca: marca!));

        Assert.Equal("A marca é obrigatória.", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ModeloNuloVazioOuEspacos_Lanca(string? modelo)
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Veiculo(modelo: modelo!));

        Assert.Equal("O modelo é obrigatório.", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_CorNulaVaziaOuEspacos_Lanca(string? cor)
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Veiculo(cor: cor!));

        Assert.Equal("A cor é obrigatória.", ex.Message);
    }

    [Theory]
    [InlineData(1949)]
    [InlineData(0)]
    [InlineData(-2000)]
    public void Construtor_AnoAbaixoDe1950_Lanca(int ano)
    {
        var ex = Assert.Throws<DomainException>(() => Fabrica.Veiculo(ano: ano));

        Assert.Equal("O ano deve ser maior ou igual a 1950.", ex.Message);
    }

    [Theory]
    [InlineData(1950)]
    [InlineData(1951)]
    public void Construtor_AnoIgualOuAcimaDe1950_Aceita(int ano)
    {
        var veiculo = Fabrica.Veiculo(ano: ano);

        Assert.Equal(ano, veiculo.Ano);
    }

    [Fact]
    public void Construtor_MarcaModeloECorComEspacosNasPontas_GuardaComTrim()
    {
        var veiculo = Fabrica.Veiculo(marca: "  Fiat ", modelo: " Argo  ", cor: "  Branco ");

        Assert.Equal("Fiat", veiculo.Marca);
        Assert.Equal("Argo", veiculo.Modelo);
        Assert.Equal("Branco", veiculo.Cor);
    }

    [Fact]
    public void Atualizar_DadosValidos_AlteraCamposECategoria()
    {
        var veiculo = Fabrica.Veiculo(placa: "ABC1D23", kmAtual: 5_000);
        var novaCategoria = Fabrica.Categoria(id: 9, nome: "Sedan");

        veiculo.Atualizar(" VW ", " Polo ", 2024, " Preto ", novaCategoria);

        Assert.Equal("VW", veiculo.Marca);
        Assert.Equal("Polo", veiculo.Modelo);
        Assert.Equal(2024, veiculo.Ano);
        Assert.Equal("Preto", veiculo.Cor);
        Assert.Same(novaCategoria, veiculo.Categoria);
        Assert.Equal(9, veiculo.CategoriaId);
        Assert.Equal("ABC1D23", veiculo.Placa);
        Assert.Equal(5_000, veiculo.KmAtual);
    }

    [Fact]
    public void Atualizar_MarcaVazia_Lanca()
    {
        var veiculo = Fabrica.Veiculo();

        var ex = Assert.Throws<DomainException>(
            () => veiculo.Atualizar(" ", "Polo", 2024, "Preto", Fabrica.Categoria()));

        Assert.Equal("A marca é obrigatória.", ex.Message);
    }

    [Fact]
    public void Atualizar_ModeloVazio_Lanca()
    {
        var veiculo = Fabrica.Veiculo();

        var ex = Assert.Throws<DomainException>(
            () => veiculo.Atualizar("VW", "", 2024, "Preto", Fabrica.Categoria()));

        Assert.Equal("O modelo é obrigatório.", ex.Message);
    }

    [Fact]
    public void Atualizar_CorVazia_Lanca()
    {
        var veiculo = Fabrica.Veiculo();

        var ex = Assert.Throws<DomainException>(
            () => veiculo.Atualizar("VW", "Polo", 2024, "", Fabrica.Categoria()));

        Assert.Equal("A cor é obrigatória.", ex.Message);
    }

    [Fact]
    public void Atualizar_Ano1949_LancaEMantemDados()
    {
        var veiculo = Fabrica.Veiculo(marca: "Fiat", ano: 2022);

        var ex = Assert.Throws<DomainException>(
            () => veiculo.Atualizar("VW", "Polo", 1949, "Preto", Fabrica.Categoria()));

        Assert.Equal("O ano deve ser maior ou igual a 1950.", ex.Message);
        Assert.Equal("Fiat", veiculo.Marca);
        Assert.Equal(2022, veiculo.Ano);
    }

    [Fact]
    public void Atualizar_Ano1950_Aceita()
    {
        var veiculo = Fabrica.Veiculo();

        veiculo.Atualizar("VW", "Fusca", 1950, "Azul", Fabrica.Categoria());

        Assert.Equal(1950, veiculo.Ano);
    }

    [Theory]
    [InlineData(StatusVeiculo.Disponivel, true, true)]
    [InlineData(StatusVeiculo.Disponivel, false, false)]
    [InlineData(StatusVeiculo.Locado, true, false)]
    [InlineData(StatusVeiculo.Manutencao, true, false)]
    public void PodeSerLocado_ConformeStatusEAtivo_DevolveEsperado(StatusVeiculo status, bool ativo, bool esperado)
    {
        var veiculo = Fabrica.Veiculo(status: status, ativo: ativo);

        Assert.Equal(esperado, veiculo.PodeSerLocado());
    }

    [Fact]
    public void PodeSerLocado_ManutencaoInativado_RetornaFalso()
    {
        var veiculo = Fabrica.Veiculo(status: StatusVeiculo.Manutencao, ativo: false);

        Assert.False(veiculo.PodeSerLocado());
    }

    [Fact]
    public void Alocar_VeiculoDisponivel_FicaLocado()
    {
        var veiculo = Fabrica.Veiculo();

        veiculo.Alocar();

        Assert.Equal(StatusVeiculo.Locado, veiculo.Status);
    }

    [Theory]
    [InlineData(StatusVeiculo.Locado, true)]
    [InlineData(StatusVeiculo.Manutencao, true)]
    [InlineData(StatusVeiculo.Disponivel, false)]
    public void Alocar_VeiculoNaoLocavel_Lanca(StatusVeiculo status, bool ativo)
    {
        var veiculo = Fabrica.Veiculo(status: status, ativo: ativo);

        var ex = Assert.Throws<DomainException>(() => veiculo.Alocar());

        Assert.Equal("Veículo indisponível para locação.", ex.Message);
    }

    [Fact]
    public void Liberar_KmMaiorQueOAtual_VoltaDisponivelEAtualizaKm()
    {
        var veiculo = Fabrica.Veiculo(kmAtual: 10_000, status: StatusVeiculo.Locado);

        veiculo.Liberar(10_350);

        Assert.Equal(StatusVeiculo.Disponivel, veiculo.Status);
        Assert.Equal(10_350, veiculo.KmAtual);
    }

    [Fact]
    public void Liberar_KmIgualAoAtual_Aceita()
    {
        var veiculo = Fabrica.Veiculo(kmAtual: 10_000, status: StatusVeiculo.Locado);

        veiculo.Liberar(10_000);

        Assert.Equal(StatusVeiculo.Disponivel, veiculo.Status);
        Assert.Equal(10_000, veiculo.KmAtual);
    }

    [Fact]
    public void Liberar_KmMenorQueOAtual_LancaEMantemLocado()
    {
        var veiculo = Fabrica.Veiculo(kmAtual: 10_000, status: StatusVeiculo.Locado);

        var ex = Assert.Throws<DomainException>(() => veiculo.Liberar(9_999));

        Assert.Equal("Km de devolução menor que o km atual do veículo.", ex.Message);
        Assert.Equal(StatusVeiculo.Locado, veiculo.Status);
        Assert.Equal(10_000, veiculo.KmAtual);
    }

    [Theory]
    [InlineData(StatusVeiculo.Disponivel)]
    [InlineData(StatusVeiculo.Manutencao)]
    public void Liberar_VeiculoNaoLocado_Lanca(StatusVeiculo status)
    {
        var veiculo = Fabrica.Veiculo(status: status);

        var ex = Assert.Throws<DomainException>(() => veiculo.Liberar(20_000));

        Assert.Equal("O veículo não está locado.", ex.Message);
    }

    [Fact]
    public void CancelarAlocacao_VeiculoLocado_VoltaDisponivelSemMexerNoKm()
    {
        var veiculo = Fabrica.Veiculo(kmAtual: 10_000, status: StatusVeiculo.Locado);

        veiculo.CancelarAlocacao();

        Assert.Equal(StatusVeiculo.Disponivel, veiculo.Status);
        Assert.Equal(10_000, veiculo.KmAtual);
    }

    [Theory]
    [InlineData(StatusVeiculo.Disponivel)]
    [InlineData(StatusVeiculo.Manutencao)]
    public void CancelarAlocacao_VeiculoNaoLocado_Lanca(StatusVeiculo status)
    {
        var veiculo = Fabrica.Veiculo(status: status);

        var ex = Assert.Throws<DomainException>(() => veiculo.CancelarAlocacao());

        Assert.Equal("O veículo não está locado.", ex.Message);
    }

    [Fact]
    public void EnviarParaManutencao_VeiculoDisponivel_FicaEmManutencao()
    {
        var veiculo = Fabrica.Veiculo();

        veiculo.EnviarParaManutencao();

        Assert.Equal(StatusVeiculo.Manutencao, veiculo.Status);
    }

    [Theory]
    [InlineData(StatusVeiculo.Locado, true)]
    [InlineData(StatusVeiculo.Manutencao, true)]
    [InlineData(StatusVeiculo.Disponivel, false)]
    public void EnviarParaManutencao_VeiculoNaoLocavel_Lanca(StatusVeiculo status, bool ativo)
    {
        var veiculo = Fabrica.Veiculo(status: status, ativo: ativo);

        var ex = Assert.Throws<DomainException>(() => veiculo.EnviarParaManutencao());

        Assert.Equal("Só veículos disponíveis podem ir para manutenção.", ex.Message);
    }

    [Fact]
    public void RetornarDaManutencao_VeiculoEmManutencao_VoltaDisponivel()
    {
        var veiculo = Fabrica.Veiculo(status: StatusVeiculo.Manutencao);

        veiculo.RetornarDaManutencao();

        Assert.Equal(StatusVeiculo.Disponivel, veiculo.Status);
    }

    [Theory]
    [InlineData(StatusVeiculo.Disponivel)]
    [InlineData(StatusVeiculo.Locado)]
    public void RetornarDaManutencao_VeiculoForaDeManutencao_Lanca(StatusVeiculo status)
    {
        var veiculo = Fabrica.Veiculo(status: status);

        var ex = Assert.Throws<DomainException>(() => veiculo.RetornarDaManutencao());

        Assert.Equal("O veículo não está em manutenção.", ex.Message);
    }

    [Fact]
    public void Inativar_VeiculoDisponivel_FicaInativo()
    {
        var veiculo = Fabrica.Veiculo();

        veiculo.Inativar();

        Assert.False(veiculo.Ativo);
    }

    [Fact]
    public void Inativar_VeiculoLocado_LancaEMantemAtivo()
    {
        var veiculo = Fabrica.Veiculo(status: StatusVeiculo.Locado);

        var ex = Assert.Throws<DomainException>(() => veiculo.Inativar());

        Assert.Equal("Não é possível inativar um veículo locado.", ex.Message);
        Assert.True(veiculo.Ativo);
    }

    [Fact]
    public void Inativar_VeiculoEmManutencao_FicaInativoMantendoStatus()
    {
        var veiculo = Fabrica.Veiculo(status: StatusVeiculo.Manutencao);

        veiculo.Inativar();

        Assert.False(veiculo.Ativo);
        Assert.Equal(StatusVeiculo.Manutencao, veiculo.Status);
    }

    [Fact]
    public void Reativar_VeiculoInativo_FicaAtivo()
    {
        var veiculo = Fabrica.Veiculo(ativo: false);

        veiculo.Reativar();

        Assert.True(veiculo.Ativo);
        Assert.True(veiculo.PodeSerLocado());
    }

    [Theory]
    [InlineData("abc-1234", "ABC1234")]
    [InlineData("abc 1d23", "ABC1D23")]
    [InlineData("ABC1D23", "ABC1D23")]
    [InlineData("", "")]
    public void NormalizarPlaca_RemoveHifenEEspacosEMaiusculiza(string placa, string esperada)
    {
        Assert.Equal(esperada, Veiculo.NormalizarPlaca(placa));
    }

    [Fact]
    public void NormalizarPlaca_Nula_RetornaVazio()
    {
        Assert.Equal(string.Empty, Veiculo.NormalizarPlaca(null));
    }
}
