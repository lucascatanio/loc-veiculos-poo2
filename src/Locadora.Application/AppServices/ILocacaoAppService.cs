using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;
using Locadora.Domain.Servicos;

namespace Locadora.Application.AppServices;

public interface ILocacaoAppService
{
    Task<List<Locacao>> ListarAsync(StatusLocacao? status, CancellationToken ct);
    Task<Locacao> ObterAsync(int id, CancellationToken ct);
    Task<Locacao> AbrirAsync(
        int clienteId, int veiculoId, DateTime dataPrevista, string? observacao,
        IReadOnlyList<EntradaPagamento> entradas, CancellationToken ct);
    Task<Locacao> RegistrarEntradaAsync(int id, EntradaPagamento entrada, CancellationToken ct);
}
