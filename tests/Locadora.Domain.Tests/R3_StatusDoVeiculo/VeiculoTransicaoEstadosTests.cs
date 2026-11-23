using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Tests.Apoio;

namespace Locadora.Domain.Tests.R3_StatusDoVeiculo;

//                        Disponivel   Locado       Manutencao          Inativo
//   Alocar               Locado       E            E                   E
//   Liberar              E            Disponivel   E                   E
//   CancelarAlocacao     E            Disponivel   E                   E
//   EnviarParaManutencao Manutencao   E            E                   E
//   RetornarDaManutencao E            E            Disponivel          E
//   Inativar             Inativo      E            Inativo/Manutencao  Inativo
//   Reativar             Disponivel   Locado       Manutencao          Disponivel
//   (E = DomainException, estado mantido; "Inativo" = Disponivel com Ativo = false)
[Trait(Tecnica.Requisito, Tecnica.StatusVeiculo)]
[Trait(Tecnica.Nome, Tecnica.TransicaoEstados)]
public class VeiculoTransicaoEstadosTests
{
    private const string Disponivel = "Disponivel";
    private const string Locado = "Locado";
    private const string Manutencao = "Manutencao";
    private const string Inativo = "Inativo";

    private static readonly Dictionary<string, Action<Veiculo>> Eventos = new()
    {
        ["Alocar"] = v => v.Alocar(),
        ["Liberar"] = v => v.Liberar(v.KmAtual + 100),
        ["CancelarAlocacao"] = v => v.CancelarAlocacao(),
        ["EnviarParaManutencao"] = v => v.EnviarParaManutencao(),
        ["RetornarDaManutencao"] = v => v.RetornarDaManutencao(),
        ["Inativar"] = v => v.Inativar(),
        ["Reativar"] = v => v.Reativar(),
    };

    private static Veiculo VeiculoEm(string estado)
    {
        var veiculo = Cenario.Veiculo();
        switch (estado)
        {
            case Locado: veiculo.Alocar(); break;
            case Manutencao: veiculo.EnviarParaManutencao(); break;
            case Inativo: veiculo.Inativar(); break;
        }
        return veiculo;
    }

    private static string Estado(Veiculo veiculo) => veiculo.Ativo
        ? veiculo.Status.ToString()
        : veiculo.Status == StatusVeiculo.Disponivel ? Inativo : $"{Inativo}/{veiculo.Status}";

    public static TheoryData<string, string, string> TransicoesValidas => new()
    {
        { Disponivel, "Alocar", Locado },
        { Disponivel, "EnviarParaManutencao", Manutencao },
        { Disponivel, "Inativar", Inativo },
        { Disponivel, "Reativar", Disponivel },
        { Locado, "Liberar", Disponivel },
        { Locado, "CancelarAlocacao", Disponivel },
        { Locado, "Reativar", Locado },
        { Manutencao, "RetornarDaManutencao", Disponivel },
        { Manutencao, "Inativar", $"{Inativo}/{Manutencao}" },
        { Manutencao, "Reativar", Manutencao },
        { Inativo, "Inativar", Inativo },
        { Inativo, "Reativar", Disponivel },
    };

    public static TheoryData<string, string> TransicoesInvalidas => new()
    {
        { Disponivel, "Liberar" },
        { Disponivel, "CancelarAlocacao" },
        { Disponivel, "RetornarDaManutencao" },
        { Locado, "Alocar" },
        { Locado, "EnviarParaManutencao" },
        { Locado, "RetornarDaManutencao" },
        { Locado, "Inativar" },
        { Manutencao, "Alocar" },
        { Manutencao, "Liberar" },
        { Manutencao, "CancelarAlocacao" },
        { Manutencao, "EnviarParaManutencao" },
        { Inativo, "Alocar" },
        { Inativo, "Liberar" },
        { Inativo, "CancelarAlocacao" },
        { Inativo, "EnviarParaManutencao" },
        { Inativo, "RetornarDaManutencao" },
    };

    [Theory]
    [MemberData(nameof(TransicoesValidas))]
    public void EventoPermitido_VaiParaEstadoEsperado(string estadoInicial, string evento, string estadoFinal)
    {
        var veiculo = VeiculoEm(estadoInicial);

        Eventos[evento](veiculo);

        Assert.Equal(estadoFinal, Estado(veiculo));
    }

    [Theory]
    [MemberData(nameof(TransicoesInvalidas))]
    public void EventoProibido_LancaExcecaoEMantemEstado(string estadoInicial, string evento)
    {
        var veiculo = VeiculoEm(estadoInicial);

        Assert.Throws<DomainException>(() => Eventos[evento](veiculo));

        Assert.Equal(estadoInicial, Estado(veiculo));
    }

    [Fact]
    public void Liberar_KmMenorQueOAtual_LancaExcecaoSemAlterarKm()
    {
        var veiculo = VeiculoEm(Locado);

        Assert.Throws<DomainException>(() => veiculo.Liberar(Cenario.KmInicial - 1));

        Assert.Equal(Cenario.KmInicial, veiculo.KmAtual);
        Assert.Equal(StatusVeiculo.Locado, veiculo.Status);
    }

    [Fact]
    public void Liberar_KmIgualAoAtual_LiberaSemAlterarKm()
    {
        var veiculo = VeiculoEm(Locado);

        veiculo.Liberar(Cenario.KmInicial);

        Assert.Equal(StatusVeiculo.Disponivel, veiculo.Status);
        Assert.Equal(Cenario.KmInicial, veiculo.KmAtual);
    }

    [Fact]
    public void Liberar_KmMaior_AtualizaKmDoVeiculo()
    {
        var veiculo = VeiculoEm(Locado);

        veiculo.Liberar(Cenario.KmInicial + 350);

        Assert.Equal(Cenario.KmInicial + 350, veiculo.KmAtual);
    }

    // PodeSerLocado: só Ativo = S e Status = Disponivel resultam em S.
    [Theory]
    [Trait(Tecnica.Nome, Tecnica.TabelaDecisao)]
    [InlineData(Disponivel, true)]
    [InlineData(Locado, false)]
    [InlineData(Manutencao, false)]
    [InlineData(Inativo, false)]
    public void PodeSerLocado_ConformeAtivoEStatus(string estado, bool esperado)
    {
        Assert.Equal(esperado, VeiculoEm(estado).PodeSerLocado());
    }
}
