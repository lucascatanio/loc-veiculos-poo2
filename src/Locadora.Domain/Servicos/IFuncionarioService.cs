using Locadora.Domain.Entidades;

namespace Locadora.Domain.Servicos;

public interface IFuncionarioService
{
    Task<Funcionario> ObterAsync(int id, CancellationToken ct);
    Task<Funcionario?> AutenticarAsync(string email, string senha, CancellationToken ct);
    Task<bool> EstaAtivoAsync(int id, CancellationToken ct);
    Task GarantirEmailDisponivelAsync(string email, int? ignorarId, CancellationToken ct);
}
