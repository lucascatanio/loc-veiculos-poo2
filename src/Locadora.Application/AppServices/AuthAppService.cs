using Locadora.Domain.Entidades;
using Locadora.Domain.Servicos;

namespace Locadora.Application.AppServices;

public class AuthAppService(IFuncionarioService funcionarioService) : IAuthAppService
{
    public Task<Funcionario?> LoginAsync(string email, string senha, CancellationToken ct) =>
        funcionarioService.AutenticarAsync(email, senha, ct);

    public Task<bool> EstaAtivoAsync(int id, CancellationToken ct) =>
        funcionarioService.EstaAtivoAsync(id, ct);
}
