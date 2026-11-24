using Locadora.Domain.Entidades;

namespace Locadora.Domain.Tests.Suporte;

public sealed class LocacaoComId : Locacao
{
    public LocacaoComId(
        int id, Cliente cliente, Veiculo veiculo, DateTime dataRetirada, DateTime dataPrevista, string? observacao)
        : base(cliente, veiculo, dataRetirada, dataPrevista, observacao) => Id = id;
}
