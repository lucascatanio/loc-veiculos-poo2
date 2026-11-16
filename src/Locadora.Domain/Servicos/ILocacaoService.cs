using Locadora.Domain.Entidades;
using Locadora.Domain.Valores;

namespace Locadora.Domain.Servicos;

public interface ILocacaoService
{
    Task<List<Locacao>> ListarAsync(string? status, CancellationToken ct);
    Task<Locacao> ObterAsync(int id, CancellationToken ct);
    Task<Locacao> AbrirAsync(
        int clienteId, int veiculoId, DateTime dataRetirada, DateTime dataPrevista, string? observacao,
        IReadOnlyList<PagamentoInformado> entradas, CancellationToken ct);
    Task<Locacao> RegistrarEntradaAsync(int id, PagamentoInformado entrada, DateTime dataPagamento, CancellationToken ct);
    Task<PreviaDevolucao> ObterPreviaAsync(int id, DateTime dataDevolucao, CancellationToken ct);
    Task<Locacao> FinalizarAsync(
        int id, DateTime dataDevolucao, int kmDevolucao, string? observacao,
        IReadOnlyList<PagamentoInformado> saidas, CancellationToken ct);
    Task<Locacao> ProrrogarAsync(int id, DateTime agora, DateTime novaDataPrevista, CancellationToken ct);
    Task<Locacao> CancelarAsync(int id, CancellationToken ct);
}
