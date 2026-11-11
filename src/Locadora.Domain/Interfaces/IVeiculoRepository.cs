using Locadora.Domain.Entidades;

namespace Locadora.Domain.Interfaces;

public interface IVeiculoRepository
{
    Task<Veiculo?> ObterPorIdAsync(int id, CancellationToken ct);
    Task<List<Veiculo>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct);
    Task<List<Veiculo>> ListarDisponiveisAsync(int? categoriaId, CancellationToken ct);
    Task<bool> ExistePlacaAsync(string placa, CancellationToken ct);
    Task<int> ContarAtivosNaCategoriaAsync(int categoriaId, CancellationToken ct);
    void Adicionar(Veiculo veiculo);
}
