using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;

namespace Locadora.Domain.Servicos;

public class FuncionarioService(IFuncionarioRepository funcionarios, IHashSenha hashSenha) : IFuncionarioService
{
    public async Task<Funcionario> ObterAsync(int id, CancellationToken ct) =>
        await funcionarios.ObterPorIdAsync(id, ct) ?? throw new NaoEncontradoException("Funcionário", id);

    public async Task<Funcionario?> AutenticarAsync(string email, string senha, CancellationToken ct)
    {
        var funcionario = await funcionarios.ObterPorEmailAsync(Funcionario.NormalizarEmail(email), ct);

        return funcionario is { Ativo: true } && funcionario.VerificarSenha(senha, hashSenha)
            ? funcionario
            : null;
    }

    public async Task<bool> EstaAtivoAsync(int id, CancellationToken ct) =>
        await funcionarios.ObterPorIdAsync(id, ct) is { Ativo: true };

    public async Task GarantirEmailDisponivelAsync(string email, int? ignorarId, CancellationToken ct)
    {
        if (await funcionarios.ExisteEmailAsync(Funcionario.NormalizarEmail(email), ignorarId, ct))
            throw new DomainException("Já existe um funcionário com este e-mail.");
    }
}
