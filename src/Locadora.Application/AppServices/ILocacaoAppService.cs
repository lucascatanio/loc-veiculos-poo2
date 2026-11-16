using Locadora.Domain.Entidades;
using Locadora.Domain.Valores;

namespace Locadora.Application.AppServices;

public interface ILocacaoAppService
{
    Task<List<Locacao>> ListarAsync(string? status, CancellationToken ct);
    Task<Locacao> ObterAsync(int id, CancellationToken ct);
    Task<Locacao> AbrirAsync(
        int clienteId, int veiculoId, DateTime dataPrevista, string? observacao,
        IReadOnlyList<PagamentoInformado> entradas, CancellationToken ct);
    Task<Locacao> RegistrarEntradaAsync(int id, PagamentoInformado entrada, CancellationToken ct);
    Task<PreviaDevolucao> ObterPreviaAsync(int id, CancellationToken ct);
    Task<Locacao> FinalizarAsync(
        int id, int kmDevolucao, string? observacao, IReadOnlyList<PagamentoInformado> saidas, CancellationToken ct);
    Task<Locacao> ProrrogarAsync(int id, DateTime novaDataPrevista, CancellationToken ct);
    Task<Locacao> CancelarAsync(int id, CancellationToken ct);
}
