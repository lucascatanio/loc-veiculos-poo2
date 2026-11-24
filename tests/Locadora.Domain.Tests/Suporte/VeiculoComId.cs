using Locadora.Domain.Entidades;

namespace Locadora.Domain.Tests.Suporte;

public sealed class VeiculoComId : Veiculo
{
    public VeiculoComId(
        int id, string placa, string marca, string modelo, int ano, string cor, int kmAtual, Categoria categoria)
        : base(placa, marca, modelo, ano, cor, kmAtual, categoria) => Id = id;
}
