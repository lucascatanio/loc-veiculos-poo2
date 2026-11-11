using Locadora.Domain.Entidades;
using Locadora.Domain.Interfaces;

namespace Locadora.Application.AppServices;

public class AuthAppService(IFuncionarioRepository funcionarios, IHashSenha hashSenha) : IAuthAppService
{
    public async Task<Funcionario?> LoginAsync(string email, string senha, CancellationToken ct)
    {
        var funcionario = await funcionarios.ObterPorEmailAsync(Funcionario.NormalizarEmail(email), ct);

        if (funcionario is null || !funcionario.Ativo || !funcionario.VerificarSenha(senha, hashSenha))
            return null;

        return funcionario;
    }

    public async Task<bool> EstaAtivoAsync(int id, CancellationToken ct) =>
        await funcionarios.ObterPorIdAsync(id, ct) is { Ativo: true };
}
