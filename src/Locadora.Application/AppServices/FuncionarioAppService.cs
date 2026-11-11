using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Servicos;

namespace Locadora.Application.AppServices;

public class FuncionarioAppService(
    IFuncionarioRepository funcionarios,
    FuncionarioService funcionarioService,
    IHashSenha hashSenha,
    IUnitOfWork unitOfWork)
{
    public Task<List<Funcionario>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct) =>
        funcionarios.ListarAsync(busca, incluirInativos, ct);

    public async Task<Funcionario> ObterAsync(int id, CancellationToken ct) =>
        await funcionarios.ObterPorIdAsync(id, ct) ?? throw new NaoEncontradoException("Funcionário", id);

    public async Task<Funcionario> CriarAsync(string nome, string email, string senha, CancellationToken ct)
    {
        Funcionario.ValidarSenha(senha);
        await funcionarioService.GarantirEmailDisponivelAsync(email, null, ct);

        var funcionario = new Funcionario(nome, email, hashSenha.Gerar(senha));
        funcionarios.Adicionar(funcionario);
        await unitOfWork.CommitAsync(ct);

        return funcionario;
    }

    public async Task<Funcionario> AtualizarAsync(int id, string nome, string email, CancellationToken ct)
    {
        var funcionario = await ObterAsync(id, ct);
        await funcionarioService.GarantirEmailDisponivelAsync(email, id, ct);

        funcionario.Atualizar(nome, email);
        await unitOfWork.CommitAsync(ct);

        return funcionario;
    }

    public async Task InativarAsync(int id, int idLogado, CancellationToken ct)
    {
        var funcionario = await ObterAsync(id, ct);

        funcionario.Inativar(idLogado);
        await unitOfWork.CommitAsync(ct);
    }

    public async Task ReativarAsync(int id, CancellationToken ct)
    {
        var funcionario = await ObterAsync(id, ct);

        funcionario.Reativar();
        await unitOfWork.CommitAsync(ct);
    }
}
