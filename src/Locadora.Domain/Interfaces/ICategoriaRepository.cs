using Locadora.Domain.Entidades;

namespace Locadora.Domain.Interfaces;

public interface ICategoriaRepository
{
    Task<Categoria?> ObterPorIdAsync(int id, CancellationToken ct);
    Task<List<Categoria>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct);
    void Adicionar(Categoria categoria);
}
