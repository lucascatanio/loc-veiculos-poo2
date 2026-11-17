using Locadora.Api.Dtos.Veiculos;

namespace Locadora.Desktop.ViewModels.Locacoes;

public sealed record VeiculoParaRetirada(int Id, string Placa, string Modelo, string CategoriaNome, decimal ValorDiaria)
{
    public static VeiculoParaRetirada De(VeiculoResponse veiculo, decimal valorDiaria) =>
        new(veiculo.Id, veiculo.Placa, veiculo.Modelo, veiculo.CategoriaNome, valorDiaria);
}
