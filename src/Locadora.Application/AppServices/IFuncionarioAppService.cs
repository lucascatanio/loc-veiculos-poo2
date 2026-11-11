using Locadora.Domain.Entidades;

namespace Locadora.Application.AppServices;

public interface IFuncionarioAppService
{
    Task<List<Funcionario>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct);
    Task<Funcionario> ObterAsync(int id, CancellationToken ct);
    Task<Funcionario> CriarAsync(string nome, string email, string senha, CancellationToken ct);
    Task<Funcionario> AtualizarAsync(int id, string nome, string email, CancellationToken ct);
    Task InativarAsync(int id, int idLogado, CancellationToken ct);
    Task ReativarAsync(int id, CancellationToken ct);
}
