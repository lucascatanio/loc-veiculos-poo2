using Locadora.Domain.Entidades;

namespace Locadora.Domain.Interfaces;

public interface ICategoriaRepository
{
    Task<Categoria?> ObterPorIdAsync(int id, CancellationToken ct);
    Task<List<Categoria>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct);
    Task<bool> ExisteNomeAsync(string nome, int? ignorarId, CancellationToken ct);
    void Adicionar(Categoria categoria);
}
