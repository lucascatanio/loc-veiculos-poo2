using Locadora.Domain.Entidades;

namespace Locadora.Application.AppServices;

public interface ICategoriaAppService
{
    Task<List<Categoria>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct);
    Task<Categoria> ObterAsync(int id, CancellationToken ct);
    Task<Categoria> CriarAsync(string nome, decimal valorDiaria, CancellationToken ct);
    Task<Categoria> AtualizarAsync(int id, string nome, decimal valorDiaria, CancellationToken ct);
    Task InativarAsync(int id, CancellationToken ct);
    Task ReativarAsync(int id, CancellationToken ct);
}
