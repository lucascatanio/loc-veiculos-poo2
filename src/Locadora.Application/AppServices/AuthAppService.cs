using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Servicos;

namespace Locadora.Application.AppServices;

public class AuthAppService(
    IFuncionarioRepository funcionarios,
    IFuncionarioService funcionarioService,
    IHashSenha hashSenha) : IAuthAppService
{
    public async Task<Funcionario?> LoginAsync(string email, string senha, CancellationToken ct)
    {
        var funcionario = await funcionarios.ObterPorEmailAsync(Funcionario.NormalizarEmail(email), ct);

        if (funcionario is null || !funcionario.Ativo || !funcionario.VerificarSenha(senha, hashSenha))
            return null;

        return funcionario;
    }

    public async Task<bool> EstaAtivoAsync(int id, CancellationToken ct)
    {
        try
        {
            var funcionario = await funcionarioService.ObterAsync(id, ct);
            return funcionario.Ativo;
        }
        catch (NaoEncontradoException)
        {
            return false;
        }
    }
}
