using Locadora.Domain.Enums;

namespace Locadora.Domain.Tests.Suporte;

public sealed class FabricaTests
{
    [Fact]
    public void Padroes_GeramEntidadesValidasComId()
    {
        var categoria = Fabrica.Categoria();
        var cliente = Fabrica.Cliente();
        var veiculo = Fabrica.Veiculo();
        var funcionario = Fabrica.Funcionario();

        Assert.Equal(1, categoria.Id);
        Assert.Equal(1, cliente.Id);
        Assert.Equal(1, veiculo.Id);
        Assert.Equal(1, veiculo.CategoriaId);
        Assert.Equal(1, funcionario.Id);
        Assert.True(categoria.Ativo && cliente.Ativo && veiculo.PodeSerLocado() && funcionario.Ativo);
    }

    [Fact]
    public void Locacao_ComDiariaERetiradaInformadas_UsaOsValoresNaLocacao()
    {
        var retirada = Fabrica.Agora.AddDays(1);

        var locacao = Fabrica.Locacao(
            id: 7, valorDiaria: 150m, kmRetirada: 42_000, retirada: retirada, prevista: retirada.AddDays(2));

        Assert.Equal(7, locacao.Id);
        Assert.Equal(StatusLocacao.Aberta, locacao.Status);
        Assert.Equal(150m, locacao.ValorDiaria);
        Assert.Equal(300m, locacao.ValorPrevisto);
        Assert.Equal(42_000, locacao.KmRetirada);
        Assert.Equal(retirada, locacao.DataRetirada);
        Assert.Equal(StatusVeiculo.Locado, locacao.Veiculo.Status);
        Assert.Equal(locacao.Cliente.Id, locacao.ClienteId);
    }

    [Fact]
    public void Estados_InativoEIndisponivel_SaoAplicados()
    {
        Assert.False(Fabrica.Cliente(ativo: false).Ativo);
        Assert.False(Fabrica.Categoria(ativo: false).Ativo);
        Assert.False(Fabrica.Funcionario(ativo: false).Ativo);
        Assert.False(Fabrica.Veiculo(ativo: false).PodeSerLocado());
        Assert.Equal(StatusVeiculo.Locado, Fabrica.Veiculo(status: StatusVeiculo.Locado).Status);
        Assert.Equal(StatusVeiculo.Manutencao, Fabrica.Veiculo(status: StatusVeiculo.Manutencao).Status);
    }

    [Fact]
    public void Pagamentos_EntradaESaida_TemOTipoCerto()
    {
        Assert.Equal(TipoPagamento.Entrada, Fabrica.Entrada().Tipo);
        Assert.Equal(TipoPagamento.Saida, Fabrica.Saida(50m, FormaPagamento.Dinheiro).Tipo);
        Assert.Equal(Fabrica.Agora, Fabrica.Entrada().DataPagamento);
    }
}
