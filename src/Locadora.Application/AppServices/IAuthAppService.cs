using Locadora.Domain.Entidades;

namespace Locadora.Application.AppServices;

public interface IAuthAppService
{
    Task<Funcionario?> LoginAsync(string email, string senha, CancellationToken ct);
    Task<bool> EstaAtivoAsync(int id, CancellationToken ct);
}
