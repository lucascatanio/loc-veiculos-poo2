using Locadora.Domain.Entidades;

namespace Locadora.Domain.Interfaces;

public interface IFuncionarioRepository
{
    Task<Funcionario?> ObterPorIdAsync(int id, CancellationToken ct);
    Task<Funcionario?> ObterPorEmailAsync(string email, CancellationToken ct);
}
