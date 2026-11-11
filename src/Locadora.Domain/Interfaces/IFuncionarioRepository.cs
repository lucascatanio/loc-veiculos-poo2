using Locadora.Domain.Entidades;

namespace Locadora.Domain.Interfaces;

public interface IFuncionarioRepository
{
    Task<Funcionario?> ObterPorIdAsync(int id, CancellationToken ct);
    Task<Funcionario?> ObterPorEmailAsync(string email, CancellationToken ct);
    Task<List<Funcionario>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct);
    Task<bool> ExisteEmailAsync(string email, int? ignorarId, CancellationToken ct);
    void Adicionar(Funcionario funcionario);
}
