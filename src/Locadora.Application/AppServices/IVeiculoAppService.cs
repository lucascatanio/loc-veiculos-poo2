using Locadora.Domain.Entidades;

namespace Locadora.Application.AppServices;

public interface IVeiculoAppService
{
    Task<List<Veiculo>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct);
    Task<List<Veiculo>> ListarDisponiveisAsync(int? categoriaId, CancellationToken ct);
    Task<Veiculo> ObterAsync(int id, CancellationToken ct);
    Task<Veiculo> CriarAsync(
        string placa, string marca, string modelo, int ano, string cor, int kmAtual, int categoriaId, CancellationToken ct);
    Task<Veiculo> AtualizarAsync(
        int id, string marca, string modelo, int ano, string cor, int categoriaId, CancellationToken ct);
    Task InativarAsync(int id, CancellationToken ct);
    Task ReativarAsync(int id, CancellationToken ct);
    Task EnviarParaManutencaoAsync(int id, CancellationToken ct);
    Task RetornarDaManutencaoAsync(int id, CancellationToken ct);
}
