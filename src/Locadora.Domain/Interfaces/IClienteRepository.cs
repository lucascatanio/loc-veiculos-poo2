using Locadora.Domain.Entidades;

namespace Locadora.Domain.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(int id, CancellationToken ct);
    Task<List<Cliente>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct);
    Task<bool> ExisteCpfAsync(string cpf, CancellationToken ct);
    void Adicionar(Cliente cliente);
}
