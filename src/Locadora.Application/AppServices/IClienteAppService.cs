using Locadora.Domain.Entidades;

namespace Locadora.Application.AppServices;

public interface IClienteAppService
{
    Task<List<Cliente>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct);
    Task<Cliente> ObterAsync(int id, CancellationToken ct);
    Task<Cliente> CriarAsync(string nome, string cpf, string cnh, string telefone, string? email, CancellationToken ct);
    Task<Cliente> AtualizarAsync(int id, string nome, string cnh, string telefone, string? email, CancellationToken ct);
    Task InativarAsync(int id, CancellationToken ct);
    Task ReativarAsync(int id, CancellationToken ct);
}
